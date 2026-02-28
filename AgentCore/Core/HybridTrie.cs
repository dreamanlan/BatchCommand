using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace CefDotnetApp.AgentCore.Core
{
    /// <summary>
    /// HybridTrie: shared trie data structures and helpers.
    /// - Supports building optimized trie from word->count map.
    /// - Supports parallelized construction (partitioned builds + merge).
    /// - Supports binary serialization and deserialization.
    /// Notes:
    /// - The serialized format stores a temp-style tree (cost + children),
    ///   then ConvertTempNode will allocate arrays/dicts according to thresholds.
    /// - After Load, the trie is ready to use (read-only).
    /// </summary>
    public class HybridTrie
    {
        // Temporary node used during build/serialization (dictionary children)
        internal class TempNode
        {
            public Dictionary<char, TempNode>? Children;
            public double Cost = double.NaN;
            public TempNode()
            {
                Children = null;
                Cost = double.NaN;
            }
            public TempNode GetOrAddChild(char c)
            {
                if (Children == null) Children = new Dictionary<char, TempNode>();
                if (!Children.TryGetValue(c, out TempNode? child))
                {
                    child = new TempNode();
                    Children[c] = child;
                }
                return child;
            }
        }

        // Optimized node after conversion (array + dict hybrid)
        public class TrieNode
        {
            public TrieNode[]? ChildrenArray;
            public Dictionary<char, TrieNode>? ChildrenDict;
            public double Cost = double.NaN;
            public bool IsWord => !double.IsNaN(Cost);
            public bool TryGetChild(char c, out TrieNode? child, int arraySize)
            {
                if (ChildrenArray != null)
                {
                    int ci = c < arraySize ? c : -1;
                    if (ci >= 0)
                    {
                        child = ChildrenArray[ci];
                        if (child != null) return true;
                    }
                }
                if (ChildrenDict != null && ChildrenDict.TryGetValue(c, out child)) return true;
                child = null;
                return false;
            }
        }

        // Public instance
        public TrieNode Root { get; internal set; } = null!;
        public double TotalCount { get; internal set; }
        public int MaxWordLen { get; internal set; }
        // Build parameters used when converting temp nodes to optimized nodes
        public int ArraySize { get; internal set; } = 256;
        public int MinChildrenForArray { get; internal set; } = 8;
        public double AsciiFractionThreshold { get; internal set; } = 0.6;

        public HybridTrie() { }

        /// <summary>
        /// Build HybridTrie from frequency lines (each line: word [sep] count).
        /// This method parallelizes parsing & aggregation, and partitions map to build subtries in parallel then merges.
        /// </summary>
        public static HybridTrie BuildFromFreqLines(
            IEnumerable<string> freqLines,
            bool caseSensitive = false,
            int arraySize = 256,
            int minChildrenForArray = 8,
            double asciiFractionThreshold = 0.6,
            int parallelPartitions = 0)
        {
            if (arraySize < 2) throw new ArgumentException("arraySize must be >= 2");
            if (minChildrenForArray < 1) minChildrenForArray = 1;
            if (asciiFractionThreshold < 0 || asciiFractionThreshold > 1) asciiFractionThreshold = 0.6;

            // Step 1: parallel aggregation into concurrent dictionary
            var counts = new ConcurrentDictionary<string, double>(caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
            var part = Partitioner.Create(freqLines ?? Enumerable.Empty<string>());

            Parallel.ForEach(part, raw =>
            {
                if (string.IsNullOrWhiteSpace(raw)) return;
                var line = raw.Trim();
                var parts = line.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) return;
                var w = parts[0];
                if (!double.TryParse(parts[1], out double cnt)) return;
                if (cnt <= 0) return;
                string key = caseSensitive ? w : w.ToLowerInvariant();
                counts.AddOrUpdate(key, cnt, (k, old) => old + cnt);
            });

            // Prepare array of pairs for partitioned building
            var pairs = counts.ToArray();
            if (pairs.Length == 0)
            {
                var emptyTrie = new HybridTrie { Root = new TrieNode(), TotalCount = 1.0, MaxWordLen = 1, ArraySize = arraySize, MinChildrenForArray = minChildrenForArray, AsciiFractionThreshold = asciiFractionThreshold };
                return emptyTrie;
            }

            double total = 0;
            int maxLen = 0;
            foreach (var kv in pairs)
            {
                total += kv.Value;
                if (kv.Key.Length > maxLen) maxLen = kv.Key.Length;
            }
            double lnTotal = Math.Log(Math.Max(total, 1.0));

            // Partition pairs into chunks for parallel sub-trie build
            int proc = parallelPartitions > 0 ? parallelPartitions : Environment.ProcessorCount;
            var chunks = PartitionArray(pairs, proc);

            var tempRoots = new TempNode[chunks.Length];
            Parallel.For(0, chunks.Length, i =>
            {
                var localRoot = new TempNode();
                foreach (var kv in chunks[i])
                {
                    string word = kv.Key;
                    double cnt = kv.Value;
                    double cost = -Math.Log(cnt) + lnTotal;
                    var node = localRoot;
                    foreach (char ch in word)
                    {
                        char nc = caseSensitive ? ch : char.ToLowerInvariant(ch);
                        node = node.GetOrAddChild(nc);
                    }
                    node.Cost = cost;
                }
                tempRoots[i] = localRoot;
            });

            // Merge temp roots into a single temp root
            var mergedRoot = new TempNode();
            foreach (var tr in tempRoots)
            {
                MergeTempNodes(mergedRoot, tr);
            }

            // Convert merged temp root to optimized TrieNode
            var hybrid = new HybridTrie();
            hybrid.ArraySize = arraySize;
            hybrid.MinChildrenForArray = minChildrenForArray;
            hybrid.AsciiFractionThreshold = asciiFractionThreshold;
            hybrid.TotalCount = Math.Max(total, 1.0);
            hybrid.MaxWordLen = Math.Max(maxLen, 1);
            hybrid.Root = hybrid.ConvertTempNode(mergedRoot);
            return hybrid;
        }

        // Partition array into nearly equal chunks
        private static KeyValuePair<string,double>[][] PartitionArray(KeyValuePair<string,double>[] arr, int partitions)
        {
            if (partitions <= 1) return new[] { arr };
            int n = arr.Length;
            var outp = new KeyValuePair<string,double>[partitions][];
            int baseSize = n / partitions;
            int rem = n % partitions;
            int idx = 0;
            for (int i = 0; i < partitions; i++)
            {
                int size = baseSize + (i < rem ? 1 : 0);
                if (size == 0) { outp[i] = new KeyValuePair<string,double>[0]; continue; }
                var chunk = new KeyValuePair<string,double>[size];
                Array.Copy(arr, idx, chunk, 0, size);
                outp[i] = chunk;
                idx += size;
            }
            return outp;
        }

        // Merge source into target (both temp nodes)
        private static void MergeTempNodes(TempNode target, TempNode source)
        {
            if (source == null) return;
            if (!double.IsNaN(source.Cost))
            {
                // If both have cost, preserve source (they should be same word from disjoint partitions)
                target.Cost = source.Cost;
            }
            if (source.Children == null) return;
            if (target.Children == null) target.Children = new Dictionary<char, TempNode>();
            foreach (var kv in source.Children)
            {
                if (!target.Children.TryGetValue(kv.Key, out TempNode? child))
                {
                    target.Children[kv.Key] = kv.Value;
                }
                else
                {
                    MergeTempNodes(child, kv.Value);
                }
            }
        }

        // Convert TempNode to optimized TrieNode using hybrid allocation heuristics
        private TrieNode ConvertTempNode(TempNode temp)
        {
            var node = new TrieNode();
            node.Cost = temp.Cost;
            if (temp.Children == null || temp.Children.Count == 0) return node;

            int childCount = temp.Children.Count;
            int asciiChildren = 0;
            foreach (var ch in temp.Children.Keys) if (ch < ArraySize) asciiChildren++;

            bool useArray = false;
            if (childCount >= MinChildrenForArray) useArray = true;
            else if (asciiChildren > 0 && ((double)asciiChildren / childCount) >= AsciiFractionThreshold) useArray = true;

            if (useArray)
            {
                node.ChildrenArray = new TrieNode[ArraySize];
                Dictionary<char, TrieNode>? dict = null;
                foreach (var kv in temp.Children)
                {
                    char c = kv.Key;
                    TrieNode childNode = ConvertTempNode(kv.Value);
                    if (c < ArraySize) node.ChildrenArray[c] = childNode;
                    else
                    {
                        if (dict == null) dict = new Dictionary<char, TrieNode>();
                        dict[c] = childNode;
                    }
                }
                node.ChildrenDict = dict;
            }
            else
            {
                var dict = new Dictionary<char, TrieNode>();
                foreach (var kv in temp.Children)
                {
                    dict[kv.Key] = ConvertTempNode(kv.Value);
                }
                node.ChildrenDict = dict;
            }
            return node;
        }

        // --------------------------
        // Serialization (binary)
        // --------------------------
        // File layout (binary):
        // Header: magic(4 bytes) 'HTR1', int arraySize, int minChildrenForArray, double asciiFractionThreshold, double totalCount, int maxWordLen
        // Then node tree serialized recursively: WriteNode(tempRoot)
        // Node format: double cost (NaN for non-terminal), int childCount, for each child: char (UTF-16) + child node
        private const string MAGIC = "HTR1";

        public void SaveToFile(string path)
        {
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            Save(fs);
        }

        public void Save(Stream stream)
        {
            using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true);
            writer.Write(MAGIC.ToCharArray());
            writer.Write(ArraySize);
            writer.Write(MinChildrenForArray);
            writer.Write(AsciiFractionThreshold);
            writer.Write(TotalCount);
            writer.Write(MaxWordLen);
            // To serialize we reconstruct a temp-style tree from optimized trie for simplicity
            var temp = RebuildTempFromOptimized(Root);
            WriteTempNode(writer, temp);
            writer.Flush();
        }

        private TempNode RebuildTempFromOptimized(TrieNode node)
        {
            var temp = new TempNode();
            temp.Cost = node.Cost;
            if (node.ChildrenArray != null)
            {
                for (int i = 0; i < node.ChildrenArray.Length; i++)
                {
                    var child = node.ChildrenArray[i];
                    if (child != null)
                    {
                        if (temp.Children == null) temp.Children = new Dictionary<char, TempNode>();
                        temp.Children[(char)i] = RebuildTempFromOptimized(child);
                    }
                }
            }
            if (node.ChildrenDict != null)
            {
                foreach (var kv in node.ChildrenDict)
                {
                    if (temp.Children == null) temp.Children = new Dictionary<char, TempNode>();
                    temp.Children[kv.Key] = RebuildTempFromOptimized(kv.Value);
                }
            }
            return temp;
        }

        private void WriteTempNode(BinaryWriter writer, TempNode node)
        {
            writer.Write(node.Cost);
            if (node.Children == null)
            {
                writer.Write(0);
                return;
            }
            writer.Write(node.Children.Count);
            foreach (var kv in node.Children)
            {
                writer.Write(kv.Key);
                WriteTempNode(writer, kv.Value);
            }
        }

        public static HybridTrie LoadFromFile(string path, int arraySize = 256, int minChildrenForArray = 8, double asciiFractionThreshold = 0.6)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Load(fs, arraySize, minChildrenForArray, asciiFractionThreshold);
        }

        public static HybridTrie Load(Stream stream, int arraySize = 256, int minChildrenForArray = 8, double asciiFractionThreshold = 0.6)
        {
            using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true);
            var magicChars = reader.ReadChars(4);
            string magic = new string(magicChars);
            if (magic != MAGIC) throw new InvalidDataException("Not a HybridTrie file or unsupported version.");
            int aSize = reader.ReadInt32();
            int minArr = reader.ReadInt32();
            double asciiFrac = reader.ReadDouble();
            double totalCount = reader.ReadDouble();
            int maxWordLen = reader.ReadInt32();

            var temp = ReadTempNode(reader);
            var hybrid = new HybridTrie();
            hybrid.ArraySize = arraySize > 0 ? arraySize : aSize;
            hybrid.MinChildrenForArray = minChildrenForArray;
            hybrid.AsciiFractionThreshold = asciiFractionThreshold;
            hybrid.TotalCount = totalCount;
            hybrid.MaxWordLen = maxWordLen;
            hybrid.Root = hybrid.ConvertTempNode(temp);
            return hybrid;
        }

        private static TempNode ReadTempNode(BinaryReader reader)
        {
            var node = new TempNode();
            node.Cost = reader.ReadDouble();
            int childCount = reader.ReadInt32();
            if (childCount <= 0) return node;
            node.Children = new Dictionary<char, TempNode>();
            for (int i = 0; i < childCount; i++)
            {
                char c = reader.ReadChar();
                var child = ReadTempNode(reader);
                node.Children[c] = child;
            }
            return node;
        }
    }
}