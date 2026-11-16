var vertexes = new[] {
	new VertexData(new Vector4(0.00f, 0.00f, 0.001f, 1.00f), new Vector4(0.50196f, 0.50196f), new Vector4(0.00f, 0.00f, 1.00f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 1.00f), new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f), new Vector4(0.00f, 0.00f, 0.00f, 0.00f), new Vector4(720.00f, 720.00f, 720.00f, 1.00f), new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f))
	, new VertexData(new Vector4(0.00f, 0.33276f, 0.001f, 1.00f), new Vector4(0.50196f, 0.16863f), new Vector4(0.00f, 0.57339f, 0.81996f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.90196f), new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f), new Vector4(0.00f, 0.00f, 0.00f, 0.00f), new Vector4(720.00f, 720.00f, 720.00f, 1.00f), new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f))
	, new VertexData(new Vector4(-0.12695f, 0.30664f, 0.001f, 1.00f), new Vector4(0.37255f, 0.19216f), new Vector4(-0.21918f, 0.55186f, 0.80431f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.90196f), new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f), new Vector4(0.00f, 0.00f, 0.00f, 0.00f), new Vector4(720.00f, 720.00f, 720.00f, 1.00f), new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f))
	, new VertexData(new Vector4(0.23535f, -0.23535f, 0.001f, 1.00f), new Vector4(0.73725f, 0.73725f), new Vector4(0.4227f, -0.40705f, 0.80822f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.90196f), new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f), new Vector4(0.00f, 0.00f, 0.00f, 0.00f), new Vector4(720.00f, 720.00f, 720.00f, 1.00f), new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f))
	, new VertexData(new Vector4(0.30664f, -0.12695f, 0.001f, 1.00f), new Vector4(0.80784f, 0.62745f), new Vector4(0.50294f, -0.21331f, 0.83757f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.90196f), new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f), new Vector4(0.00f, 0.00f, 0.00f, 0.00f), new Vector4(720.00f, 720.00f, 720.00f, 1.00f), new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f))
	, new VertexData(new Vector4(0.00f, -0.33276f, 0.001f, 1.00f), new Vector4(0.50196f, 0.83137f), new Vector4(0.00f, -0.57339f, 0.81996f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.90196f), new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f), new Vector4(0.00f, 0.00f, 0.00f, 0.00f), new Vector4(720.00f, 720.00f, 720.00f, 1.00f), new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f))
	, new VertexData(new Vector4(0.12695f, -0.30664f, 0.001f, 1.00f), new Vector4(0.62745f, 0.80784f), new Vector4(0.22114f, -0.5225f, 0.82387f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.90196f), new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f), new Vector4(0.00f, 0.00f, 0.00f, 0.00f), new Vector4(720.00f, 720.00f, 720.00f, 1.00f), new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f))
	, new VertexData(new Vector4(-0.23535f, -0.23535f, 0.001f, 1.00f), new Vector4(0.26275f, 0.73725f), new Vector4(-0.40117f, -0.40509f, 0.82192f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.90196f), new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f), new Vector4(0.00f, 0.00f, 0.00f, 0.00f), new Vector4(720.00f, 720.00f, 720.00f, 1.00f), new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f))
	, new VertexData(new Vector4(-0.12695f, -0.30664f, 0.001f, 1.00f), new Vector4(0.37255f, 0.80784f), new Vector4(-0.22114f, -0.5225f, 0.82387f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.90196f), new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f), new Vector4(0.00f, 0.00f, 0.00f, 0.00f), new Vector4(720.00f, 720.00f, 720.00f, 1.00f), new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f))
	, new VertexData(new Vector4(-0.33276f, 0.00f, 0.001f, 1.00f), new Vector4(0.16863f, 0.50196f), new Vector4(-0.57339f, 0.00f, 0.81996f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.90196f), new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f), new Vector4(0.00f, 0.00f, 0.00f, 0.00f), new Vector4(720.00f, 720.00f, 720.00f, 1.00f), new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f))
	, new VertexData(new Vector4(-0.30664f, -0.12695f, 0.001f, 1.00f), new Vector4(0.19216f, 0.62745f), new Vector4(-0.52446f, -0.21722f, 0.82387f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.90196f), new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f), new Vector4(0.00f, 0.00f, 0.00f, 0.00f), new Vector4(720.00f, 720.00f, 720.00f, 1.00f), new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f))
	, new VertexData(new Vector4(-0.23535f, 0.23535f, 0.001f, 1.00f), new Vector4(0.26275f, 0.26275f), new Vector4(-0.40313f, 0.39922f, 0.82387f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.90196f), new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f), new Vector4(0.00f, 0.00f, 0.00f, 0.00f), new Vector4(720.00f, 720.00f, 720.00f, 1.00f), new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f))
	, new VertexData(new Vector4(-0.30664f, 0.12695f, 0.001f, 1.00f), new Vector4(0.19216f, 0.37255f), new Vector4(-0.52446f, 0.21722f, 0.82387f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.90196f), new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f), new Vector4(0.00f, 0.00f, 0.00f, 0.00f), new Vector4(720.00f, 720.00f, 720.00f, 1.00f), new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f))
	, new VertexData(new Vector4(0.23535f, 0.23535f, 0.001f, 1.00f), new Vector4(0.73725f, 0.26275f), new Vector4(0.4227f, 0.4227f, 0.80235f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.90196f), new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f), new Vector4(0.00f, 0.00f, 0.00f, 0.00f), new Vector4(720.00f, 720.00f, 720.00f, 1.00f), new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f))
	, new VertexData(new Vector4(0.12695f, 0.30664f, 0.001f, 1.00f), new Vector4(0.62745f, 0.19216f), new Vector4(0.21722f, 0.54795f, 0.80822f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.90196f), new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f), new Vector4(0.00f, 0.00f, 0.00f, 0.00f), new Vector4(720.00f, 720.00f, 720.00f, 1.00f), new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f))
	, new VertexData(new Vector4(0.33276f, 0.00f, 0.001f, 1.00f), new Vector4(0.83137f, 0.50196f), new Vector4(0.57339f, 0.00f, 0.81996f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.90196f), new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f), new Vector4(0.00f, 0.00f, 0.00f, 0.00f), new Vector4(720.00f, 720.00f, 720.00f, 1.00f), new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f))
	, new VertexData(new Vector4(0.30664f, 0.12695f, 0.001f, 1.00f), new Vector4(0.80784f, 0.37255f), new Vector4(0.50294f, 0.21331f, 0.83757f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.90196f), new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f), new Vector4(0.00f, 0.00f, 0.00f, 0.00f), new Vector4(720.00f, 720.00f, 720.00f, 1.00f), new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f))
	, new VertexData(new Vector4(0.49976f, 0.00f, 0.001f, 1.00f), new Vector4(1.00f, 0.50196f), new Vector4(0.94716f, 0.00f, 0.3229f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.00f), new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f), new Vector4(0.00f, 0.00f, 0.00f, 0.00f), new Vector4(720.00f, 720.00f, 720.00f, 1.00f), new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f))
	, new VertexData(new Vector4(0.00f, 0.49976f, 0.001f, 1.00f), new Vector4(0.50196f, 0.00f), new Vector4(0.00f, 0.94716f, 0.3229f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.00f), new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f), new Vector4(0.00f, 0.00f, 0.00f, 0.00f), new Vector4(720.00f, 720.00f, 720.00f, 1.00f), new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f))
	, new VertexData(new Vector4(0.35352f, -0.35352f, 0.001f, 1.00f), new Vector4(0.8549f, 0.8549f), new Vector4(0.66928f, -0.66928f, 0.3229f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.00f), new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f), new Vector4(0.00f, 0.00f, 0.00f, 0.00f), new Vector4(720.00f, 720.00f, 720.00f, 1.00f), new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f))
	, new VertexData(new Vector4(0.46069f, -0.1908f, 0.001f, 1.00f), new Vector4(0.96078f, 0.6902f), new Vector4(0.87476f, -0.36204f, 0.32485f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.00f), new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f), new Vector4(0.00f, 0.00f, 0.00f, 0.00f), new Vector4(720.00f, 720.00f, 720.00f, 1.00f), new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f))
	, new VertexData(new Vector4(0.00f, -0.49976f, 0.001f, 1.00f), new Vector4(0.50196f, 1.00f), new Vector4(0.00f, -0.94716f, 0.3229f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.00f), new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f), new Vector4(0.00f, 0.00f, 0.00f, 0.00f), new Vector4(720.00f, 720.00f, 720.00f, 1.00f), new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f))
	, new VertexData(new Vector4(-0.49976f, 0.00f, 0.001f, 1.00f), new Vector4(0.00f, 0.50196f), new Vector4(-0.94716f, 0.00f, 0.3229f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.00f), new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f), new Vector4(0.00f, 0.00f, 0.00f, 0.00f), new Vector4(720.00f, 720.00f, 720.00f, 1.00f), new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f))
	, new VertexData(new Vector4(-0.46069f, -0.1908f, 0.001f, 1.00f), new Vector4(0.03922f, 0.6902f), new Vector4(-0.87476f, -0.36204f, 0.32485f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.00f), new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f), new Vector4(0.00f, 0.00f, 0.00f, 0.00f), new Vector4(720.00f, 720.00f, 720.00f, 1.00f), new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f))
	, new VertexData(new Vector4(-0.35352f, -0.35352f, 0.001f, 1.00f), new Vector4(0.1451f, 0.8549f), new Vector4(-0.66928f, -0.66928f, 0.3229f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.00f), new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f), new Vector4(0.00f, 0.00f, 0.00f, 0.00f), new Vector4(720.00f, 720.00f, 720.00f, 1.00f), new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f))
	, new VertexData(new Vector4(0.46069f, 0.1908f, 0.001f, 1.00f), new Vector4(0.96078f, 0.3098f), new Vector4(0.87476f, 0.36204f, 0.32485f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.00f), new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f), new Vector4(0.00f, 0.00f, 0.00f, 0.00f), new Vector4(720.00f, 720.00f, 720.00f, 1.00f), new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f))
	, new VertexData(new Vector4(-0.1908f, 0.46069f, 0.001f, 1.00f), new Vector4(0.3098f, 0.03922f), new Vector4(-0.36204f, 0.87476f, 0.32485f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.00f), new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f), new Vector4(0.00f, 0.00f, 0.00f, 0.00f), new Vector4(720.00f, 720.00f, 720.00f, 1.00f), new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f))
	, new VertexData(new Vector4(0.1908f, 0.46069f, 0.001f, 1.00f), new Vector4(0.6902f, 0.03922f), new Vector4(0.36204f, 0.87476f, 0.32485f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.00f), new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f), new Vector4(0.00f, 0.00f, 0.00f, 0.00f), new Vector4(720.00f, 720.00f, 720.00f, 1.00f), new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f))
	, new VertexData(new Vector4(0.1908f, -0.46069f, 0.001f, 1.00f), new Vector4(0.6902f, 0.96078f), new Vector4(0.36204f, -0.87476f, 0.32485f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.00f), new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f), new Vector4(0.00f, 0.00f, 0.00f, 0.00f), new Vector4(720.00f, 720.00f, 720.00f, 1.00f), new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f))
	, new VertexData(new Vector4(-0.46069f, 0.1908f, 0.001f, 1.00f), new Vector4(0.03922f, 0.3098f), new Vector4(-0.87476f, 0.36204f, 0.32485f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.00f), new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f), new Vector4(0.00f, 0.00f, 0.00f, 0.00f), new Vector4(720.00f, 720.00f, 720.00f, 1.00f), new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f))
	, new VertexData(new Vector4(-0.35352f, 0.35352f, 0.001f, 1.00f), new Vector4(0.1451f, 0.1451f), new Vector4(-0.66928f, 0.66928f, 0.3229f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.00f), new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f), new Vector4(0.00f, 0.00f, 0.00f, 0.00f), new Vector4(720.00f, 720.00f, 720.00f, 1.00f), new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f))
	, new VertexData(new Vector4(-0.1908f, -0.46069f, 0.001f, 1.00f), new Vector4(0.3098f, 0.96078f), new Vector4(-0.36204f, -0.87476f, 0.32485f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.00f), new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f), new Vector4(0.00f, 0.00f, 0.00f, 0.00f), new Vector4(720.00f, 720.00f, 720.00f, 1.00f), new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f))
	, new VertexData(new Vector4(0.35352f, 0.35352f, 0.001f, 1.00f), new Vector4(0.8549f, 0.1451f), new Vector4(0.66928f, 0.66928f, 0.3229f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.00f), new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f), new Vector4(0.00f, 0.00f, 0.00f, 0.00f), new Vector4(720.00f, 720.00f, 720.00f, 1.00f), new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f))
};
var vertexes = new[] {
	new VertexData(new Vector4(-867.74292f, -201.18993f, 489.66693f, 489.86304f), new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f), new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f), new Vector4(0.60233f, 1.42053f, 0.50196f, 0.50196f), new Vector4(-188.93994f, 345.52649f, 489.76498f, 489.86304f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 1.00f), new Vector4(0.00f, 1.00f, 0.00f, 1.00f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(0.70066f, 0.20f, 0.00f, 0.00f), new Vector4(0.10241f, 0.10241f, 0.10241f, 1.00f), new Vector4(0.00111f, 0.00877f, 0.00901f, 0.2585f))
	, new VertexData(new Vector4(-1061.31482f, -394.80603f, 336.36371f, 336.56104f), new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f), new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f), new Vector4(0.09182f, 0.99178f, 0.50196f, 0.16863f), new Vector4(-362.37689f, 365.68353f, 336.46237f, 336.56104f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.90196f), new Vector4(0.00f, 0.81996f, -0.57339f, 1.00f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(0.7003f, 0.20f, 0.00f, 0.00f), new Vector4(0.12705f, 0.12705f, 0.12705f, 1.00f), new Vector4(0.00111f, 0.00877f, 0.00901f, 0.19392f))
	, new VertexData(new Vector4(-964.24146f, -446.23126f, 295.6456f, 295.84326f), new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f), new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f), new Vector4(-0.0386f, 1.22025f, 0.37255f, 0.19216f), new Vector4(-334.1991f, 371.03726f, 295.74445f, 295.84326f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.90196f), new Vector4(-0.21918f, 0.80431f, -0.55186f, 1.00f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(0.69801f, 0.20f, 0.00f, 0.00f), new Vector4(0.13531f, 0.13531f, 0.13531f, 1.00f), new Vector4(0.00111f, 0.00877f, 0.00901f, 0.17567f))
	, new VertexData(new Vector4(-882.62445f, 59.2598f, 695.88806f, 696.08252f), new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f), new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f), new Vector4(1.26534f, 1.36282f, 0.73725f, 0.73725f), new Vector4(-93.27097f, 318.41135f, 695.98529f, 696.08252f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.90196f), new Vector4(0.4227f, 0.80822f, 0.40705f, 1.00f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(0.70066f, 0.20f, 0.00f, 0.00f), new Vector4(0.08638f, 0.08638f, 0.08638f, 1.00f), new Vector4(0.00111f, 0.00877f, 0.00901f, 0.33595f))
	, new VertexData(new Vector4(-991.6582f, 33.60112f, 675.57196f, 675.7666f), new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f), new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f), new Vector4(1.18796f, 1.11347f, 0.80784f, 0.62745f), new Vector4(-157.9458f, 321.08273f, 675.66931f, 675.7666f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.90196f), new Vector4(0.50294f, 0.83757f, 0.21331f, 1.00f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(0.70066f, 0.20f, 0.00f, 0.00f), new Vector4(0.08753f, 0.08753f, 0.08753f, 1.00f), new Vector4(0.00111f, 0.00877f, 0.00901f, 0.32881f))
	, new VertexData(new Vector4(-674.17102f, -7.57358f, 642.97015f, 643.16504f), new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f), new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f), new Vector4(1.10684f, 1.84423f, 0.50196f, 0.83137f), new Vector4(-15.50299f, 325.36929f, 643.06763f, 643.16504f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.90196f), new Vector4(0.00f, 0.81996f, 0.57339f, 1.00f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(0.70066f, 0.20f, 0.00f, 0.00f), new Vector4(0.08952f, 0.08952f, 0.08952f, 1.00f), new Vector4(0.00111f, 0.00877f, 0.00901f, 0.31708f))
	, new VertexData(new Vector4(-771.24426f, 43.8514f, 683.68799f, 683.88257f), new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f), new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f), new Vector4(1.23221f, 1.62178f, 0.62745f, 0.80784f), new Vector4(-43.68085f, 320.01559f, 683.78528f, 683.88257f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.90196f), new Vector4(0.22114f, 0.82387f, 0.5225f, 1.00f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(0.70066f, 0.20f, 0.00f, 0.00f), new Vector4(0.08706f, 0.08706f, 0.08706f, 1.00f), new Vector4(0.00111f, 0.00877f, 0.00901f, 0.33155f))
	, new VertexData(new Vector4(-579.04852f, -187.76425f, 500.29715f, 500.49316f), new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f), new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f), new Vector4(0.655f, 2.08954f, 0.26275f, 0.73725f), new Vector4(-39.27768f, 344.12872f, 500.39514f, 500.49316f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.90196f), new Vector4(-0.40117f, 0.82192f, 0.40509f, 1.00f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(0.70066f, 0.20f, 0.00f, 0.00f), new Vector4(0.10126f, 0.10126f, 0.10126f, 1.00f), new Vector4(0.00111f, 0.00877f, 0.00901f, 0.26277f))
	, new VertexData(new Vector4(-607.48962f, -89.39783f, 578.18256f, 578.37793f), new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f), new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f), new Vector4(0.90434f, 2.01217f, 0.37255f, 0.80784f), new Vector4(-14.55585f, 333.88788f, 578.28027f, 578.37793f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.90196f), new Vector4(-0.22114f, 0.82387f, 0.5225f, 1.00f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(0.70066f, 0.20f, 0.00f, 0.00f), new Vector4(0.09413f, 0.09413f, 0.09413f, 1.00f), new Vector4(0.00111f, 0.00877f, 0.00901f, 0.29306f))
	, new VertexData(new Vector4(-653.12976f, -375.82352f, 351.39386f, 351.59106f), new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f), new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f), new Vector4(0.17358f, 1.93104f, 0.16863f, 0.50196f), new Vector4(-150.76935f, 363.70728f, 351.49246f, 351.59106f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.90196f), new Vector4(-0.57339f, 0.81996f, 0.00f, 1.00f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(0.70054f, 0.20f, 0.00f, 0.00f), new Vector4(0.12369f, 0.12369f, 0.12369f, 1.00f), new Vector4(0.00111f, 0.00877f, 0.00901f, 0.20056f))
	, new VertexData(new Vector4(-596.12769f, -288.24744f, 420.73572f, 420.93237f), new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f), new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f), new Vector4(0.39604f, 2.05642f, 0.19216f, 0.62745f), new Vector4(-87.59766f, 354.5899f, 420.83405f, 420.93237f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.90196f), new Vector4(-0.52446f, 0.82387f, 0.21722f, 1.00f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(0.70066f, 0.20f, 0.00f, 0.00f), new Vector4(0.11127f, 0.11127f, 0.11127f, 1.00f), new Vector4(0.00111f, 0.00877f, 0.00901f, 0.23029f))
	, new VertexData(new Vector4(-852.86121f, -461.63968f, 283.44556f, 283.64331f), new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f), new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f), new Vector4(-0.07172f, 1.47921f, 0.26275f, 0.26275f), new Vector4(-284.60895f, 372.64148f, 283.54443f, 283.64331f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.90196f), new Vector4(-0.40313f, 0.82387f, -0.39922f, 1.00f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(0.69653f, 0.20f, 0.00f, 0.00f), new Vector4(0.13531f, 0.13531f, 0.13531f, 1.00f), new Vector4(0.00111f, 0.00877f, 0.00901f, 0.17009f))
	, new VertexData(new Vector4(-743.82745f, -435.98099f, 303.7619f, 303.95947f), new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f), new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f), new Vector4(0.00565f, 1.72855f, 0.19216f, 0.37255f), new Vector4(-219.93399f, 369.97021f, 303.86069f, 303.95947f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.90196f), new Vector4(-0.52446f, 0.82387f, -0.21722f, 1.00f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(0.69875f, 0.20f, 0.00f, 0.00f), new Vector4(0.13531f, 0.13531f, 0.13531f, 1.00f), new Vector4(0.00111f, 0.00877f, 0.00901f, 0.17936f))
	, new VertexData(new Vector4(-1156.43713f, -214.61562f, 479.03647f, 479.23267f), new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f), new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f), new Vector4(0.53861f, 0.75248f, 0.73725f, 0.26275f), new Vector4(-338.60223f, 346.92413f, 479.13458f, 479.23267f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.90196f), new Vector4(0.4227f, 0.80235f, -0.4227f, 1.00f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(0.70066f, 0.20f, 0.00f, 0.00f), new Vector4(0.10361f, 0.10361f, 0.10361f, 1.00f), new Vector4(0.00111f, 0.00877f, 0.00901f, 0.25428f))
	, new VertexData(new Vector4(-1127.99609f, -312.98178f, 401.15109f, 401.3479f), new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f), new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f), new Vector4(0.28927f, 0.82986f, 0.62745f, 0.19216f), new Vector4(-363.3241f, 357.16486f, 401.24951f, 401.3479f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.90196f), new Vector4(0.21722f, 0.80822f, -0.54795f, 1.00f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(0.70066f, 0.20f, 0.00f, 0.00f), new Vector4(0.11434f, 0.11434f, 0.11434f, 1.00f), new Vector4(0.00111f, 0.00877f, 0.00901f, 0.222f))
	, new VertexData(new Vector4(-1082.35583f, -26.55634f, 627.93976f, 628.13477f), new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f), new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f), new Vector4(1.02604f, 0.91603f, 0.83137f, 0.50196f), new Vector4(-227.11053f, 327.34555f, 628.03723f, 628.13477f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.90196f), new Vector4(0.57339f, 0.81996f, 0.00f, 1.00f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(0.70066f, 0.20f, 0.00f, 0.00f), new Vector4(0.0905f, 0.0905f, 0.0905f, 1.00f), new Vector4(0.00111f, 0.00877f, 0.00901f, 0.31146f))
	, new VertexData(new Vector4(-1139.35803f, -114.13243f, 558.59814f, 558.7937f), new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f), new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f), new Vector4(0.79757f, 0.78561f, 0.80784f, 0.37255f), new Vector4(-290.28217f, 336.46307f, 558.69592f, 558.7937f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.90196f), new Vector4(0.50294f, 0.83757f, -0.21331f, 1.00f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(0.70066f, 0.20f, 0.00f, 0.00f), new Vector4(0.09574f, 0.09574f, 0.09574f, 1.00f), new Vector4(0.00111f, 0.00877f, 0.00901f, 0.2855f))
	, new VertexData(new Vector4(-1190.05615f, 61.08101f, 697.32996f, 697.52441f), new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f), new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f), new Vector4(1.24293f, 0.65777f, 1.00f, 0.50196f), new Vector4(-246.26587f, 318.22171f, 697.42719f, 697.52441f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.00f), new Vector4(0.94716f, 0.3229f, 0.00f, 1.00f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(0.70066f, 0.20f, 0.00f, 0.00f), new Vector4(0.0863f, 0.0863f, 0.0863f, 1.00f), new Vector4(0.00111f, 0.00877f, 0.00901f, 0.33645f))
	, new VertexData(new Vector4(-1158.45581f, -491.9693f, 259.43073f, 259.62866f), new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f), new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f), new Vector4(-0.16643f, 0.77489f, 0.50196f, 0.00f), new Vector4(-449.41357f, 375.79898f, 259.52969f, 259.62866f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.00f), new Vector4(0.00f, 0.3229f, -0.94716f, 1.00f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(0.69191f, 0.20f, 0.00f, 0.00f), new Vector4(0.13531f, 0.13531f, 0.13531f, 1.00f), new Vector4(0.00111f, 0.00877f, 0.00901f, 0.15902f))
	, new VertexData(new Vector4(-890.09613f, 190.02499f, 799.42651f, 799.62012f), new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f), new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f), new Vector4(1.59684f, 1.33396f, 0.8549f, 0.8549f), new Vector4(-45.23801f, 304.79755f, 799.52332f, 799.62012f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.00f), new Vector4(0.66928f, 0.3229f, 0.66928f, 1.00f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(0.70066f, 0.20f, 0.00f, 0.00f), new Vector4(0.08145f, 0.08145f, 0.08145f, 1.00f), new Vector4(0.00111f, 0.00877f, 0.00901f, 0.37095f))
	, new VertexData(new Vector4(-1053.87524f, 151.59428f, 768.99756f, 769.19141f), new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f), new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f), new Vector4(1.48078f, 0.95995f, 0.96078f, 0.6902f), new Vector4(-142.34192f, 308.79855f, 769.09448f, 769.19141f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.00f), new Vector4(0.87476f, 0.32485f, 0.36204f, 1.00f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(0.70066f, 0.20f, 0.00f, 0.00f), new Vector4(0.08276f, 0.08276f, 0.08276f, 1.00f), new Vector4(0.00111f, 0.00877f, 0.00901f, 0.36088f))
	, new VertexData(new Vector4(-577.02991f, 89.58945f, 719.90289f, 720.09717f), new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f), new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f), new Vector4(1.36509f, 2.06113f, 0.50196f, 1.00f), new Vector4(71.53363f, 315.25385f, 720.00f, 720.09717f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.00f), new Vector4(0.00f, 0.3229f, 0.94716f, 1.00f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(0.70066f, 0.20f, 0.00f, 0.00f), new Vector4(0.08511f, 0.08511f, 0.08511f, 1.00f), new Vector4(0.00111f, 0.00877f, 0.00901f, 0.34416f))
	, new VertexData(new Vector4(-545.42969f, -463.46063f, 282.00366f, 282.20142f), new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f), new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f), new Vector4(-0.04331f, 2.1893f, 0.00f, 0.50196f), new Vector4(-131.61414f, 372.83102f, 282.10254f, 282.20142f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.00f), new Vector4(-0.94716f, 0.3229f, 0.00f, 1.00f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(0.69633f, 0.20f, 0.00f, 0.00f), new Vector4(0.13531f, 0.13531f, 0.13531f, 1.00f), new Vector4(0.00111f, 0.00877f, 0.00901f, 0.16945f))
	, new VertexData(new Vector4(-459.63474f, -331.94754f, 386.13437f, 386.3313f), new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f), new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f), new Vector4(0.29542f, 2.37135f, 0.03922f, 0.6902f), new Vector4(-36.65172f, 359.1394f, 386.23285f, 386.3313f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.00f), new Vector4(-0.87476f, 0.32485f, 0.36204f, 1.00f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(0.70066f, 0.20f, 0.00f, 0.00f), new Vector4(0.11691f, 0.11691f, 0.11691f, 1.00f), new Vector4(0.00111f, 0.00877f, 0.00901f, 0.21562f))
	, new VertexData(new Vector4(-434.10251f, -181.02367f, 505.63434f, 505.83032f), new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f), new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f), new Vector4(0.68386f, 2.42104f, 0.1451f, 0.8549f), new Vector4(35.86391f, 343.427f, 505.73233f, 505.83032f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.00f), new Vector4(-0.66928f, 0.3229f, 0.66928f, 1.00f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(0.70066f, 0.20f, 0.00f, 0.00f), new Vector4(0.1007f, 0.1007f, 0.1007f, 1.00f), new Vector4(0.00111f, 0.00877f, 0.00901f, 0.26486f))
	, new VertexData(new Vector4(-1275.85083f, -70.43234f, 593.19928f, 593.39453f), new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f), new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f), new Vector4(0.8982f, 0.47067f, 0.96078f, 0.3098f), new Vector4(-341.22815f, 331.91345f, 593.29688f, 593.39453f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.00f), new Vector4(0.87476f, 0.32485f, -0.36204f, 1.00f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(0.70066f, 0.20f, 0.00f, 0.00f), new Vector4(0.09297f, 0.09297f, 0.09297f, 1.00f), new Vector4(0.00111f, 0.00877f, 0.00901f, 0.29861f))
	, new VertexData(new Vector4(-1012.6806f, -569.37024f, 198.14558f, 198.34399f), new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f), new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f), new Vector4(-0.35354f, 1.11962f, 0.3098f, 0.03922f), new Vector4(-407.1683f, 383.85712f, 198.24478f, 198.34399f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.00f), new Vector4(-0.36204f, 0.32485f, -0.87476f, 1.00f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(0.66342f, 0.20f, 0.00f, 0.00f), new Vector4(0.13531f, 0.13531f, 0.13531f, 1.00f), new Vector4(0.00111f, 0.00877f, 0.00901f, 0.12999f))
	, new VertexData(new Vector4(-1258.78491f, -369.11172f, 356.7081f, 356.90527f), new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f), new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f), new Vector4(0.13574f, 0.53704f, 0.6902f, 0.03922f), new Vector4(-450.93982f, 363.00848f, 356.8067f, 356.90527f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.00f), new Vector4(0.36204f, 0.32485f, -0.87476f, 1.00f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(0.70058f, 0.20f, 0.00f, 0.00f), new Vector4(0.12256f, 0.12256f, 0.12256f, 1.00f), new Vector4(0.00111f, 0.00877f, 0.00901f, 0.20289f))
	, new VertexData(new Vector4(-722.80511f, 166.99065f, 781.18805f, 781.38184f), new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f), new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f), new Vector4(1.54715f, 1.7224f, 0.6902f, 0.96078f), new Vector4(29.28836f, 307.19559f, 781.28491f, 781.38184f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.00f), new Vector4(0.36204f, 0.32485f, 0.87476f, 1.00f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(0.70066f, 0.20f, 0.00f, 0.00f), new Vector4(0.08223f, 0.08223f, 0.08223f, 1.00f), new Vector4(0.00111f, 0.00877f, 0.00901f, 0.36493f))
	, new VertexData(new Vector4(-681.61047f, -553.97412f, 210.33611f, 210.53442f), new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f), new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f), new Vector4(-0.28716f, 1.88208f, 0.03922f, 0.3098f), new Vector4(-235.53802f, 382.25427f, 210.43527f, 210.53442f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.00f), new Vector4(-0.87476f, 0.32485f, -0.36204f, 1.00f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(0.67165f, 0.20f, 0.00f, 0.00f), new Vector4(0.13531f, 0.13531f, 0.13531f, 1.00f), new Vector4(0.00111f, 0.00877f, 0.00901f, 0.13586f))
	, new VertexData(new Vector4(-845.38971f, -592.40485f, 179.90715f, 180.10571f), new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f), new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f), new Vector4(-0.40323f, 1.50806f, 0.1451f, 0.1451f), new Vector4(-332.642f, 386.25528f, 180.00644f, 180.10571f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.00f), new Vector4(-0.66928f, 0.3229f, -0.66928f, 1.00f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(0.64802f, 0.20f, 0.00f, 0.00f), new Vector4(0.13531f, 0.13531f, 0.13531f, 1.00f), new Vector4(0.00111f, 0.00877f, 0.00901f, 0.12113f))
	, new VertexData(new Vector4(-476.70074f, -33.26787f, 622.62555f, 622.82056f), new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f), new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f), new Vector4(1.05787f, 2.30498f, 0.3098f, 0.96078f), new Vector4(73.05991f, 328.04422f, 622.72302f, 622.82056f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.00f), new Vector4(-0.36204f, 0.32485f, 0.87476f, 1.00f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(0.70066f, 0.20f, 0.00f, 0.00f), new Vector4(0.09086f, 0.09086f, 0.09086f, 1.00f), new Vector4(0.00111f, 0.00877f, 0.00901f, 0.30949f))
	, new VertexData(new Vector4(-1301.3833f, -221.3562f, 473.69952f, 473.89575f), new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f), new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f), new Vector4(0.50976f, 0.42098f, 0.8549f, 0.1451f), new Vector4(-413.74377f, 347.62598f, 473.79764f, 473.89575f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(1.00f, 1.00f, 1.00f, 0.00f), new Vector4(0.66928f, 0.3229f, -0.66928f, 1.00f), new Vector4(1.00f, 0.00f, 0.00f, 1.00f), new Vector4(0.70066f, 0.20f, 0.00f, 0.00f), new Vector4(0.10423f, 0.10423f, 0.10423f, 1.00f), new Vector4(0.00111f, 0.00877f, 0.00901f, 0.25198f))
};

// gl_Position
var gl_Position = new[] {
	new Vector4(-867.74292f, -201.18993f, 489.66693f, 489.86304f)
	, new Vector4(-1061.31482f, -394.80603f, 336.36371f, 336.56104f)
	, new Vector4(-964.24146f, -446.23126f, 295.6456f, 295.84326f)
	, new Vector4(-882.62445f, 59.2598f, 695.88806f, 696.08252f)
	, new Vector4(-991.6582f, 33.60112f, 675.57196f, 675.7666f)
	, new Vector4(-674.17102f, -7.57358f, 642.97015f, 643.16504f)
	, new Vector4(-771.24426f, 43.8514f, 683.68799f, 683.88257f)
	, new Vector4(-579.04852f, -187.76425f, 500.29715f, 500.49316f)
	, new Vector4(-607.48962f, -89.39783f, 578.18256f, 578.37793f)
	, new Vector4(-653.12976f, -375.82352f, 351.39386f, 351.59106f)
	, new Vector4(-596.12769f, -288.24744f, 420.73572f, 420.93237f)
	, new Vector4(-852.86121f, -461.63968f, 283.44556f, 283.64331f)
	, new Vector4(-743.82745f, -435.98099f, 303.7619f, 303.95947f)
	, new Vector4(-1156.43713f, -214.61562f, 479.03647f, 479.23267f)
	, new Vector4(-1127.99609f, -312.98178f, 401.15109f, 401.3479f)
	, new Vector4(-1082.35583f, -26.55634f, 627.93976f, 628.13477f)
	, new Vector4(-1139.35803f, -114.13243f, 558.59814f, 558.7937f)
	, new Vector4(-1190.05615f, 61.08101f, 697.32996f, 697.52441f)
	, new Vector4(-1158.45581f, -491.9693f, 259.43073f, 259.62866f)
	, new Vector4(-890.09613f, 190.02499f, 799.42651f, 799.62012f)
	, new Vector4(-1053.87524f, 151.59428f, 768.99756f, 769.19141f)
	, new Vector4(-577.02991f, 89.58945f, 719.90289f, 720.09717f)
	, new Vector4(-545.42969f, -463.46063f, 282.00366f, 282.20142f)
	, new Vector4(-459.63474f, -331.94754f, 386.13437f, 386.3313f)
	, new Vector4(-434.10251f, -181.02367f, 505.63434f, 505.83032f)
	, new Vector4(-1275.85083f, -70.43234f, 593.19928f, 593.39453f)
	, new Vector4(-1012.6806f, -569.37024f, 198.14558f, 198.34399f)
	, new Vector4(-1258.78491f, -369.11172f, 356.7081f, 356.90527f)
	, new Vector4(-722.80511f, 166.99065f, 781.18805f, 781.38184f)
	, new Vector4(-681.61047f, -553.97412f, 210.33611f, 210.53442f)
	, new Vector4(-845.38971f, -592.40485f, 179.90715f, 180.10571f)
	, new Vector4(-476.70074f, -33.26787f, 622.62555f, 622.82056f)
	, new Vector4(-1301.3833f, -221.3562f, 473.69952f, 473.89575f)
};

// in_attr0
var in_attr0 = new[] {
	new Vector4(0.00f, 0.00f, 0.001f, 1.00f)
	, new Vector4(0.00f, 0.33276f, 0.001f, 1.00f)
	, new Vector4(-0.12695f, 0.30664f, 0.001f, 1.00f)
	, new Vector4(0.23535f, -0.23535f, 0.001f, 1.00f)
	, new Vector4(0.30664f, -0.12695f, 0.001f, 1.00f)
	, new Vector4(0.00f, -0.33276f, 0.001f, 1.00f)
	, new Vector4(0.12695f, -0.30664f, 0.001f, 1.00f)
	, new Vector4(-0.23535f, -0.23535f, 0.001f, 1.00f)
	, new Vector4(-0.12695f, -0.30664f, 0.001f, 1.00f)
	, new Vector4(-0.33276f, 0.00f, 0.001f, 1.00f)
	, new Vector4(-0.30664f, -0.12695f, 0.001f, 1.00f)
	, new Vector4(-0.23535f, 0.23535f, 0.001f, 1.00f)
	, new Vector4(-0.30664f, 0.12695f, 0.001f, 1.00f)
	, new Vector4(0.23535f, 0.23535f, 0.001f, 1.00f)
	, new Vector4(0.12695f, 0.30664f, 0.001f, 1.00f)
	, new Vector4(0.33276f, 0.00f, 0.001f, 1.00f)
	, new Vector4(0.30664f, 0.12695f, 0.001f, 1.00f)
	, new Vector4(0.49976f, 0.00f, 0.001f, 1.00f)
	, new Vector4(0.00f, 0.49976f, 0.001f, 1.00f)
	, new Vector4(0.35352f, -0.35352f, 0.001f, 1.00f)
	, new Vector4(0.46069f, -0.1908f, 0.001f, 1.00f)
	, new Vector4(0.00f, -0.49976f, 0.001f, 1.00f)
	, new Vector4(-0.49976f, 0.00f, 0.001f, 1.00f)
	, new Vector4(-0.46069f, -0.1908f, 0.001f, 1.00f)
	, new Vector4(-0.35352f, -0.35352f, 0.001f, 1.00f)
	, new Vector4(0.46069f, 0.1908f, 0.001f, 1.00f)
	, new Vector4(-0.1908f, 0.46069f, 0.001f, 1.00f)
	, new Vector4(0.1908f, 0.46069f, 0.001f, 1.00f)
	, new Vector4(0.1908f, -0.46069f, 0.001f, 1.00f)
	, new Vector4(-0.46069f, 0.1908f, 0.001f, 1.00f)
	, new Vector4(-0.35352f, 0.35352f, 0.001f, 1.00f)
	, new Vector4(-0.1908f, -0.46069f, 0.001f, 1.00f)
	, new Vector4(0.35352f, 0.35352f, 0.001f, 1.00f)
};

// in_attr1
var in_attr1 = new[] {
	new Vector4(0.50196f, 0.50196f)
	, new Vector4(0.50196f, 0.16863f)
	, new Vector4(0.37255f, 0.19216f)
	, new Vector4(0.73725f, 0.73725f)
	, new Vector4(0.80784f, 0.62745f)
	, new Vector4(0.50196f, 0.83137f)
	, new Vector4(0.62745f, 0.80784f)
	, new Vector4(0.26275f, 0.73725f)
	, new Vector4(0.37255f, 0.80784f)
	, new Vector4(0.16863f, 0.50196f)
	, new Vector4(0.19216f, 0.62745f)
	, new Vector4(0.26275f, 0.26275f)
	, new Vector4(0.19216f, 0.37255f)
	, new Vector4(0.73725f, 0.26275f)
	, new Vector4(0.62745f, 0.19216f)
	, new Vector4(0.83137f, 0.50196f)
	, new Vector4(0.80784f, 0.37255f)
	, new Vector4(1.00f, 0.50196f)
	, new Vector4(0.50196f, 0.00f)
	, new Vector4(0.8549f, 0.8549f)
	, new Vector4(0.96078f, 0.6902f)
	, new Vector4(0.50196f, 1.00f)
	, new Vector4(0.00f, 0.50196f)
	, new Vector4(0.03922f, 0.6902f)
	, new Vector4(0.1451f, 0.8549f)
	, new Vector4(0.96078f, 0.3098f)
	, new Vector4(0.3098f, 0.03922f)
	, new Vector4(0.6902f, 0.03922f)
	, new Vector4(0.6902f, 0.96078f)
	, new Vector4(0.03922f, 0.3098f)
	, new Vector4(0.1451f, 0.1451f)
	, new Vector4(0.3098f, 0.96078f)
	, new Vector4(0.8549f, 0.1451f)
};

// in_attr2
var in_attr2 = new[] {
	new Vector4(0.00f, 0.00f, 1.00f, 1.00f)
	, new Vector4(0.00f, 0.57339f, 0.81996f, 1.00f)
	, new Vector4(-0.21918f, 0.55186f, 0.80431f, 1.00f)
	, new Vector4(0.4227f, -0.40705f, 0.80822f, 1.00f)
	, new Vector4(0.50294f, -0.21331f, 0.83757f, 1.00f)
	, new Vector4(0.00f, -0.57339f, 0.81996f, 1.00f)
	, new Vector4(0.22114f, -0.5225f, 0.82387f, 1.00f)
	, new Vector4(-0.40117f, -0.40509f, 0.82192f, 1.00f)
	, new Vector4(-0.22114f, -0.5225f, 0.82387f, 1.00f)
	, new Vector4(-0.57339f, 0.00f, 0.81996f, 1.00f)
	, new Vector4(-0.52446f, -0.21722f, 0.82387f, 1.00f)
	, new Vector4(-0.40313f, 0.39922f, 0.82387f, 1.00f)
	, new Vector4(-0.52446f, 0.21722f, 0.82387f, 1.00f)
	, new Vector4(0.4227f, 0.4227f, 0.80235f, 1.00f)
	, new Vector4(0.21722f, 0.54795f, 0.80822f, 1.00f)
	, new Vector4(0.57339f, 0.00f, 0.81996f, 1.00f)
	, new Vector4(0.50294f, 0.21331f, 0.83757f, 1.00f)
	, new Vector4(0.94716f, 0.00f, 0.3229f, 1.00f)
	, new Vector4(0.00f, 0.94716f, 0.3229f, 1.00f)
	, new Vector4(0.66928f, -0.66928f, 0.3229f, 1.00f)
	, new Vector4(0.87476f, -0.36204f, 0.32485f, 1.00f)
	, new Vector4(0.00f, -0.94716f, 0.3229f, 1.00f)
	, new Vector4(-0.94716f, 0.00f, 0.3229f, 1.00f)
	, new Vector4(-0.87476f, -0.36204f, 0.32485f, 1.00f)
	, new Vector4(-0.66928f, -0.66928f, 0.3229f, 1.00f)
	, new Vector4(0.87476f, 0.36204f, 0.32485f, 1.00f)
	, new Vector4(-0.36204f, 0.87476f, 0.32485f, 1.00f)
	, new Vector4(0.36204f, 0.87476f, 0.32485f, 1.00f)
	, new Vector4(0.36204f, -0.87476f, 0.32485f, 1.00f)
	, new Vector4(-0.87476f, 0.36204f, 0.32485f, 1.00f)
	, new Vector4(-0.66928f, 0.66928f, 0.3229f, 1.00f)
	, new Vector4(-0.36204f, -0.87476f, 0.32485f, 1.00f)
	, new Vector4(0.66928f, 0.66928f, 0.3229f, 1.00f)
};

// in_attr3
var in_attr3 = new[] {
	new Vector4(1.00f, 1.00f, 1.00f, 1.00f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.90196f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.90196f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.90196f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.90196f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.90196f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.90196f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.90196f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.90196f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.90196f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.90196f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.90196f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.90196f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.90196f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.90196f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.90196f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.90196f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.00f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.00f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.00f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.00f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.00f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.00f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.00f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.00f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.00f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.00f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.00f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.00f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.00f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.00f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.00f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.00f)
};

// in_attr4
var in_attr4 = new[] {
	new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f)
	, new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f)
	, new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f)
	, new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f)
	, new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f)
	, new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f)
	, new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f)
	, new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f)
	, new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f)
	, new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f)
	, new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f)
	, new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f)
	, new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f)
	, new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f)
	, new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f)
	, new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f)
	, new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f)
	, new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f)
	, new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f)
	, new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f)
	, new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f)
	, new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f)
	, new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f)
	, new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f)
	, new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f)
	, new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f)
	, new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f)
	, new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f)
	, new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f)
	, new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f)
	, new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f)
	, new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f)
	, new Vector4(0.00f, 0.00f, 0.00f, 2.68435E+08f)
};

// in_attr5
var in_attr5 = new[] {
	new Vector4(0.00f, 0.00f, 0.00f, 0.00f)
	, new Vector4(0.00f, 0.00f, 0.00f, 0.00f)
	, new Vector4(0.00f, 0.00f, 0.00f, 0.00f)
	, new Vector4(0.00f, 0.00f, 0.00f, 0.00f)
	, new Vector4(0.00f, 0.00f, 0.00f, 0.00f)
	, new Vector4(0.00f, 0.00f, 0.00f, 0.00f)
	, new Vector4(0.00f, 0.00f, 0.00f, 0.00f)
	, new Vector4(0.00f, 0.00f, 0.00f, 0.00f)
	, new Vector4(0.00f, 0.00f, 0.00f, 0.00f)
	, new Vector4(0.00f, 0.00f, 0.00f, 0.00f)
	, new Vector4(0.00f, 0.00f, 0.00f, 0.00f)
	, new Vector4(0.00f, 0.00f, 0.00f, 0.00f)
	, new Vector4(0.00f, 0.00f, 0.00f, 0.00f)
	, new Vector4(0.00f, 0.00f, 0.00f, 0.00f)
	, new Vector4(0.00f, 0.00f, 0.00f, 0.00f)
	, new Vector4(0.00f, 0.00f, 0.00f, 0.00f)
	, new Vector4(0.00f, 0.00f, 0.00f, 0.00f)
	, new Vector4(0.00f, 0.00f, 0.00f, 0.00f)
	, new Vector4(0.00f, 0.00f, 0.00f, 0.00f)
	, new Vector4(0.00f, 0.00f, 0.00f, 0.00f)
	, new Vector4(0.00f, 0.00f, 0.00f, 0.00f)
	, new Vector4(0.00f, 0.00f, 0.00f, 0.00f)
	, new Vector4(0.00f, 0.00f, 0.00f, 0.00f)
	, new Vector4(0.00f, 0.00f, 0.00f, 0.00f)
	, new Vector4(0.00f, 0.00f, 0.00f, 0.00f)
	, new Vector4(0.00f, 0.00f, 0.00f, 0.00f)
	, new Vector4(0.00f, 0.00f, 0.00f, 0.00f)
	, new Vector4(0.00f, 0.00f, 0.00f, 0.00f)
	, new Vector4(0.00f, 0.00f, 0.00f, 0.00f)
	, new Vector4(0.00f, 0.00f, 0.00f, 0.00f)
	, new Vector4(0.00f, 0.00f, 0.00f, 0.00f)
	, new Vector4(0.00f, 0.00f, 0.00f, 0.00f)
	, new Vector4(0.00f, 0.00f, 0.00f, 0.00f)
};

// in_attr6
var in_attr6 = new[] {
	new Vector4(720.00f, 720.00f, 720.00f, 1.00f)
	, new Vector4(720.00f, 720.00f, 720.00f, 1.00f)
	, new Vector4(720.00f, 720.00f, 720.00f, 1.00f)
	, new Vector4(720.00f, 720.00f, 720.00f, 1.00f)
	, new Vector4(720.00f, 720.00f, 720.00f, 1.00f)
	, new Vector4(720.00f, 720.00f, 720.00f, 1.00f)
	, new Vector4(720.00f, 720.00f, 720.00f, 1.00f)
	, new Vector4(720.00f, 720.00f, 720.00f, 1.00f)
	, new Vector4(720.00f, 720.00f, 720.00f, 1.00f)
	, new Vector4(720.00f, 720.00f, 720.00f, 1.00f)
	, new Vector4(720.00f, 720.00f, 720.00f, 1.00f)
	, new Vector4(720.00f, 720.00f, 720.00f, 1.00f)
	, new Vector4(720.00f, 720.00f, 720.00f, 1.00f)
	, new Vector4(720.00f, 720.00f, 720.00f, 1.00f)
	, new Vector4(720.00f, 720.00f, 720.00f, 1.00f)
	, new Vector4(720.00f, 720.00f, 720.00f, 1.00f)
	, new Vector4(720.00f, 720.00f, 720.00f, 1.00f)
	, new Vector4(720.00f, 720.00f, 720.00f, 1.00f)
	, new Vector4(720.00f, 720.00f, 720.00f, 1.00f)
	, new Vector4(720.00f, 720.00f, 720.00f, 1.00f)
	, new Vector4(720.00f, 720.00f, 720.00f, 1.00f)
	, new Vector4(720.00f, 720.00f, 720.00f, 1.00f)
	, new Vector4(720.00f, 720.00f, 720.00f, 1.00f)
	, new Vector4(720.00f, 720.00f, 720.00f, 1.00f)
	, new Vector4(720.00f, 720.00f, 720.00f, 1.00f)
	, new Vector4(720.00f, 720.00f, 720.00f, 1.00f)
	, new Vector4(720.00f, 720.00f, 720.00f, 1.00f)
	, new Vector4(720.00f, 720.00f, 720.00f, 1.00f)
	, new Vector4(720.00f, 720.00f, 720.00f, 1.00f)
	, new Vector4(720.00f, 720.00f, 720.00f, 1.00f)
	, new Vector4(720.00f, 720.00f, 720.00f, 1.00f)
	, new Vector4(720.00f, 720.00f, 720.00f, 1.00f)
	, new Vector4(720.00f, 720.00f, 720.00f, 1.00f)
};

// in_attr7
var in_attr7 = new[] {
	new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f)
	, new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f)
	, new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f)
	, new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f)
	, new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f)
	, new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f)
	, new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f)
	, new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f)
	, new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f)
	, new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f)
	, new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f)
	, new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f)
	, new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f)
	, new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f)
	, new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f)
	, new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f)
	, new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f)
	, new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f)
	, new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f)
	, new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f)
	, new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f)
	, new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f)
	, new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f)
	, new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f)
	, new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f)
	, new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f)
	, new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f)
	, new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f)
	, new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f)
	, new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f)
	, new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f)
	, new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f)
	, new Vector4(0.5484f, 0.96051f, 0.58835f, 0.42892f)
};

// out_attr0
var out_attr0 = new[] {
	new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f)
	, new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f)
	, new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f)
	, new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f)
	, new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f)
	, new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f)
	, new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f)
	, new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f)
	, new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f)
	, new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f)
	, new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f)
	, new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f)
	, new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f)
	, new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f)
	, new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f)
	, new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f)
	, new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f)
	, new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f)
	, new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f)
	, new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f)
	, new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f)
	, new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f)
	, new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f)
	, new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f)
	, new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f)
	, new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f)
	, new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f)
	, new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f)
	, new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f)
	, new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f)
	, new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f)
	, new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f)
	, new Vector4(0.25425f, 0.27381f, 0.27335f, 1.50f)
};

// out_attr1
var out_attr1 = new[] {
	new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f)
	, new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f)
	, new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f)
	, new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f)
	, new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f)
	, new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f)
	, new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f)
	, new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f)
	, new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f)
	, new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f)
	, new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f)
	, new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f)
	, new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f)
	, new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f)
	, new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f)
	, new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f)
	, new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f)
	, new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f)
	, new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f)
	, new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f)
	, new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f)
	, new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f)
	, new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f)
	, new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f)
	, new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f)
	, new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f)
	, new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f)
	, new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f)
	, new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f)
	, new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f)
	, new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f)
	, new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f)
	, new Vector4(0.1291f, 0.14181f, 0.14524f, 1.00f)
};

// out_attr10
var out_attr10 = new[] {
	new Vector4(0.00111f, 0.00877f, 0.00901f, 0.2585f)
	, new Vector4(0.00111f, 0.00877f, 0.00901f, 0.19392f)
	, new Vector4(0.00111f, 0.00877f, 0.00901f, 0.17567f)
	, new Vector4(0.00111f, 0.00877f, 0.00901f, 0.33595f)
	, new Vector4(0.00111f, 0.00877f, 0.00901f, 0.32881f)
	, new Vector4(0.00111f, 0.00877f, 0.00901f, 0.31708f)
	, new Vector4(0.00111f, 0.00877f, 0.00901f, 0.33155f)
	, new Vector4(0.00111f, 0.00877f, 0.00901f, 0.26277f)
	, new Vector4(0.00111f, 0.00877f, 0.00901f, 0.29306f)
	, new Vector4(0.00111f, 0.00877f, 0.00901f, 0.20056f)
	, new Vector4(0.00111f, 0.00877f, 0.00901f, 0.23029f)
	, new Vector4(0.00111f, 0.00877f, 0.00901f, 0.17009f)
	, new Vector4(0.00111f, 0.00877f, 0.00901f, 0.17936f)
	, new Vector4(0.00111f, 0.00877f, 0.00901f, 0.25428f)
	, new Vector4(0.00111f, 0.00877f, 0.00901f, 0.222f)
	, new Vector4(0.00111f, 0.00877f, 0.00901f, 0.31146f)
	, new Vector4(0.00111f, 0.00877f, 0.00901f, 0.2855f)
	, new Vector4(0.00111f, 0.00877f, 0.00901f, 0.33645f)
	, new Vector4(0.00111f, 0.00877f, 0.00901f, 0.15902f)
	, new Vector4(0.00111f, 0.00877f, 0.00901f, 0.37095f)
	, new Vector4(0.00111f, 0.00877f, 0.00901f, 0.36088f)
	, new Vector4(0.00111f, 0.00877f, 0.00901f, 0.34416f)
	, new Vector4(0.00111f, 0.00877f, 0.00901f, 0.16945f)
	, new Vector4(0.00111f, 0.00877f, 0.00901f, 0.21562f)
	, new Vector4(0.00111f, 0.00877f, 0.00901f, 0.26486f)
	, new Vector4(0.00111f, 0.00877f, 0.00901f, 0.29861f)
	, new Vector4(0.00111f, 0.00877f, 0.00901f, 0.12999f)
	, new Vector4(0.00111f, 0.00877f, 0.00901f, 0.20289f)
	, new Vector4(0.00111f, 0.00877f, 0.00901f, 0.36493f)
	, new Vector4(0.00111f, 0.00877f, 0.00901f, 0.13586f)
	, new Vector4(0.00111f, 0.00877f, 0.00901f, 0.12113f)
	, new Vector4(0.00111f, 0.00877f, 0.00901f, 0.30949f)
	, new Vector4(0.00111f, 0.00877f, 0.00901f, 0.25198f)
};

// out_attr2
var out_attr2 = new[] {
	new Vector4(0.60233f, 1.42053f, 0.50196f, 0.50196f)
	, new Vector4(0.09182f, 0.99178f, 0.50196f, 0.16863f)
	, new Vector4(-0.0386f, 1.22025f, 0.37255f, 0.19216f)
	, new Vector4(1.26534f, 1.36282f, 0.73725f, 0.73725f)
	, new Vector4(1.18796f, 1.11347f, 0.80784f, 0.62745f)
	, new Vector4(1.10684f, 1.84423f, 0.50196f, 0.83137f)
	, new Vector4(1.23221f, 1.62178f, 0.62745f, 0.80784f)
	, new Vector4(0.655f, 2.08954f, 0.26275f, 0.73725f)
	, new Vector4(0.90434f, 2.01217f, 0.37255f, 0.80784f)
	, new Vector4(0.17358f, 1.93104f, 0.16863f, 0.50196f)
	, new Vector4(0.39604f, 2.05642f, 0.19216f, 0.62745f)
	, new Vector4(-0.07172f, 1.47921f, 0.26275f, 0.26275f)
	, new Vector4(0.00565f, 1.72855f, 0.19216f, 0.37255f)
	, new Vector4(0.53861f, 0.75248f, 0.73725f, 0.26275f)
	, new Vector4(0.28927f, 0.82986f, 0.62745f, 0.19216f)
	, new Vector4(1.02604f, 0.91603f, 0.83137f, 0.50196f)
	, new Vector4(0.79757f, 0.78561f, 0.80784f, 0.37255f)
	, new Vector4(1.24293f, 0.65777f, 1.00f, 0.50196f)
	, new Vector4(-0.16643f, 0.77489f, 0.50196f, 0.00f)
	, new Vector4(1.59684f, 1.33396f, 0.8549f, 0.8549f)
	, new Vector4(1.48078f, 0.95995f, 0.96078f, 0.6902f)
	, new Vector4(1.36509f, 2.06113f, 0.50196f, 1.00f)
	, new Vector4(-0.04331f, 2.1893f, 0.00f, 0.50196f)
	, new Vector4(0.29542f, 2.37135f, 0.03922f, 0.6902f)
	, new Vector4(0.68386f, 2.42104f, 0.1451f, 0.8549f)
	, new Vector4(0.8982f, 0.47067f, 0.96078f, 0.3098f)
	, new Vector4(-0.35354f, 1.11962f, 0.3098f, 0.03922f)
	, new Vector4(0.13574f, 0.53704f, 0.6902f, 0.03922f)
	, new Vector4(1.54715f, 1.7224f, 0.6902f, 0.96078f)
	, new Vector4(-0.28716f, 1.88208f, 0.03922f, 0.3098f)
	, new Vector4(-0.40323f, 1.50806f, 0.1451f, 0.1451f)
	, new Vector4(1.05787f, 2.30498f, 0.3098f, 0.96078f)
	, new Vector4(0.50976f, 0.42098f, 0.8549f, 0.1451f)
};

// out_attr3
var out_attr3 = new[] {
	new Vector4(-188.93994f, 345.52649f, 489.76498f, 489.86304f)
	, new Vector4(-362.37689f, 365.68353f, 336.46237f, 336.56104f)
	, new Vector4(-334.1991f, 371.03726f, 295.74445f, 295.84326f)
	, new Vector4(-93.27097f, 318.41135f, 695.98529f, 696.08252f)
	, new Vector4(-157.9458f, 321.08273f, 675.66931f, 675.7666f)
	, new Vector4(-15.50299f, 325.36929f, 643.06763f, 643.16504f)
	, new Vector4(-43.68085f, 320.01559f, 683.78528f, 683.88257f)
	, new Vector4(-39.27768f, 344.12872f, 500.39514f, 500.49316f)
	, new Vector4(-14.55585f, 333.88788f, 578.28027f, 578.37793f)
	, new Vector4(-150.76935f, 363.70728f, 351.49246f, 351.59106f)
	, new Vector4(-87.59766f, 354.5899f, 420.83405f, 420.93237f)
	, new Vector4(-284.60895f, 372.64148f, 283.54443f, 283.64331f)
	, new Vector4(-219.93399f, 369.97021f, 303.86069f, 303.95947f)
	, new Vector4(-338.60223f, 346.92413f, 479.13458f, 479.23267f)
	, new Vector4(-363.3241f, 357.16486f, 401.24951f, 401.3479f)
	, new Vector4(-227.11053f, 327.34555f, 628.03723f, 628.13477f)
	, new Vector4(-290.28217f, 336.46307f, 558.69592f, 558.7937f)
	, new Vector4(-246.26587f, 318.22171f, 697.42719f, 697.52441f)
	, new Vector4(-449.41357f, 375.79898f, 259.52969f, 259.62866f)
	, new Vector4(-45.23801f, 304.79755f, 799.52332f, 799.62012f)
	, new Vector4(-142.34192f, 308.79855f, 769.09448f, 769.19141f)
	, new Vector4(71.53363f, 315.25385f, 720.00f, 720.09717f)
	, new Vector4(-131.61414f, 372.83102f, 282.10254f, 282.20142f)
	, new Vector4(-36.65172f, 359.1394f, 386.23285f, 386.3313f)
	, new Vector4(35.86391f, 343.427f, 505.73233f, 505.83032f)
	, new Vector4(-341.22815f, 331.91345f, 593.29688f, 593.39453f)
	, new Vector4(-407.1683f, 383.85712f, 198.24478f, 198.34399f)
	, new Vector4(-450.93982f, 363.00848f, 356.8067f, 356.90527f)
	, new Vector4(29.28836f, 307.19559f, 781.28491f, 781.38184f)
	, new Vector4(-235.53802f, 382.25427f, 210.43527f, 210.53442f)
	, new Vector4(-332.642f, 386.25528f, 180.00644f, 180.10571f)
	, new Vector4(73.05991f, 328.04422f, 622.72302f, 622.82056f)
	, new Vector4(-413.74377f, 347.62598f, 473.79764f, 473.89575f)
};

// out_attr4
var out_attr4 = new[] {
	new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
};

// out_attr5
var out_attr5 = new[] {
	new Vector4(1.00f, 1.00f, 1.00f, 1.00f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.90196f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.90196f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.90196f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.90196f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.90196f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.90196f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.90196f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.90196f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.90196f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.90196f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.90196f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.90196f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.90196f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.90196f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.90196f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.90196f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.00f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.00f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.00f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.00f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.00f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.00f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.00f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.00f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.00f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.00f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.00f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.00f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.00f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.00f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.00f)
	, new Vector4(1.00f, 1.00f, 1.00f, 0.00f)
};

// out_attr6
var out_attr6 = new[] {
	new Vector4(0.00f, 1.00f, 0.00f, 1.00f)
	, new Vector4(0.00f, 0.81996f, -0.57339f, 1.00f)
	, new Vector4(-0.21918f, 0.80431f, -0.55186f, 1.00f)
	, new Vector4(0.4227f, 0.80822f, 0.40705f, 1.00f)
	, new Vector4(0.50294f, 0.83757f, 0.21331f, 1.00f)
	, new Vector4(0.00f, 0.81996f, 0.57339f, 1.00f)
	, new Vector4(0.22114f, 0.82387f, 0.5225f, 1.00f)
	, new Vector4(-0.40117f, 0.82192f, 0.40509f, 1.00f)
	, new Vector4(-0.22114f, 0.82387f, 0.5225f, 1.00f)
	, new Vector4(-0.57339f, 0.81996f, 0.00f, 1.00f)
	, new Vector4(-0.52446f, 0.82387f, 0.21722f, 1.00f)
	, new Vector4(-0.40313f, 0.82387f, -0.39922f, 1.00f)
	, new Vector4(-0.52446f, 0.82387f, -0.21722f, 1.00f)
	, new Vector4(0.4227f, 0.80235f, -0.4227f, 1.00f)
	, new Vector4(0.21722f, 0.80822f, -0.54795f, 1.00f)
	, new Vector4(0.57339f, 0.81996f, 0.00f, 1.00f)
	, new Vector4(0.50294f, 0.83757f, -0.21331f, 1.00f)
	, new Vector4(0.94716f, 0.3229f, 0.00f, 1.00f)
	, new Vector4(0.00f, 0.3229f, -0.94716f, 1.00f)
	, new Vector4(0.66928f, 0.3229f, 0.66928f, 1.00f)
	, new Vector4(0.87476f, 0.32485f, 0.36204f, 1.00f)
	, new Vector4(0.00f, 0.3229f, 0.94716f, 1.00f)
	, new Vector4(-0.94716f, 0.3229f, 0.00f, 1.00f)
	, new Vector4(-0.87476f, 0.32485f, 0.36204f, 1.00f)
	, new Vector4(-0.66928f, 0.3229f, 0.66928f, 1.00f)
	, new Vector4(0.87476f, 0.32485f, -0.36204f, 1.00f)
	, new Vector4(-0.36204f, 0.32485f, -0.87476f, 1.00f)
	, new Vector4(0.36204f, 0.32485f, -0.87476f, 1.00f)
	, new Vector4(0.36204f, 0.32485f, 0.87476f, 1.00f)
	, new Vector4(-0.87476f, 0.32485f, -0.36204f, 1.00f)
	, new Vector4(-0.66928f, 0.3229f, -0.66928f, 1.00f)
	, new Vector4(-0.36204f, 0.32485f, 0.87476f, 1.00f)
	, new Vector4(0.66928f, 0.3229f, -0.66928f, 1.00f)
};

// out_attr7
var out_attr7 = new[] {
	new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
	, new Vector4(1.00f, 0.00f, 0.00f, 1.00f)
};

// out_attr8
var out_attr8 = new[] {
	new Vector4(0.70066f, 0.20f, 0.00f, 0.00f)
	, new Vector4(0.7003f, 0.20f, 0.00f, 0.00f)
	, new Vector4(0.69801f, 0.20f, 0.00f, 0.00f)
	, new Vector4(0.70066f, 0.20f, 0.00f, 0.00f)
	, new Vector4(0.70066f, 0.20f, 0.00f, 0.00f)
	, new Vector4(0.70066f, 0.20f, 0.00f, 0.00f)
	, new Vector4(0.70066f, 0.20f, 0.00f, 0.00f)
	, new Vector4(0.70066f, 0.20f, 0.00f, 0.00f)
	, new Vector4(0.70066f, 0.20f, 0.00f, 0.00f)
	, new Vector4(0.70054f, 0.20f, 0.00f, 0.00f)
	, new Vector4(0.70066f, 0.20f, 0.00f, 0.00f)
	, new Vector4(0.69653f, 0.20f, 0.00f, 0.00f)
	, new Vector4(0.69875f, 0.20f, 0.00f, 0.00f)
	, new Vector4(0.70066f, 0.20f, 0.00f, 0.00f)
	, new Vector4(0.70066f, 0.20f, 0.00f, 0.00f)
	, new Vector4(0.70066f, 0.20f, 0.00f, 0.00f)
	, new Vector4(0.70066f, 0.20f, 0.00f, 0.00f)
	, new Vector4(0.70066f, 0.20f, 0.00f, 0.00f)
	, new Vector4(0.69191f, 0.20f, 0.00f, 0.00f)
	, new Vector4(0.70066f, 0.20f, 0.00f, 0.00f)
	, new Vector4(0.70066f, 0.20f, 0.00f, 0.00f)
	, new Vector4(0.70066f, 0.20f, 0.00f, 0.00f)
	, new Vector4(0.69633f, 0.20f, 0.00f, 0.00f)
	, new Vector4(0.70066f, 0.20f, 0.00f, 0.00f)
	, new Vector4(0.70066f, 0.20f, 0.00f, 0.00f)
	, new Vector4(0.70066f, 0.20f, 0.00f, 0.00f)
	, new Vector4(0.66342f, 0.20f, 0.00f, 0.00f)
	, new Vector4(0.70058f, 0.20f, 0.00f, 0.00f)
	, new Vector4(0.70066f, 0.20f, 0.00f, 0.00f)
	, new Vector4(0.67165f, 0.20f, 0.00f, 0.00f)
	, new Vector4(0.64802f, 0.20f, 0.00f, 0.00f)
	, new Vector4(0.70066f, 0.20f, 0.00f, 0.00f)
	, new Vector4(0.70066f, 0.20f, 0.00f, 0.00f)
};

// out_attr9
var out_attr9 = new[] {
	new Vector4(0.10241f, 0.10241f, 0.10241f, 1.00f)
	, new Vector4(0.12705f, 0.12705f, 0.12705f, 1.00f)
	, new Vector4(0.13531f, 0.13531f, 0.13531f, 1.00f)
	, new Vector4(0.08638f, 0.08638f, 0.08638f, 1.00f)
	, new Vector4(0.08753f, 0.08753f, 0.08753f, 1.00f)
	, new Vector4(0.08952f, 0.08952f, 0.08952f, 1.00f)
	, new Vector4(0.08706f, 0.08706f, 0.08706f, 1.00f)
	, new Vector4(0.10126f, 0.10126f, 0.10126f, 1.00f)
	, new Vector4(0.09413f, 0.09413f, 0.09413f, 1.00f)
	, new Vector4(0.12369f, 0.12369f, 0.12369f, 1.00f)
	, new Vector4(0.11127f, 0.11127f, 0.11127f, 1.00f)
	, new Vector4(0.13531f, 0.13531f, 0.13531f, 1.00f)
	, new Vector4(0.13531f, 0.13531f, 0.13531f, 1.00f)
	, new Vector4(0.10361f, 0.10361f, 0.10361f, 1.00f)
	, new Vector4(0.11434f, 0.11434f, 0.11434f, 1.00f)
	, new Vector4(0.0905f, 0.0905f, 0.0905f, 1.00f)
	, new Vector4(0.09574f, 0.09574f, 0.09574f, 1.00f)
	, new Vector4(0.0863f, 0.0863f, 0.0863f, 1.00f)
	, new Vector4(0.13531f, 0.13531f, 0.13531f, 1.00f)
	, new Vector4(0.08145f, 0.08145f, 0.08145f, 1.00f)
	, new Vector4(0.08276f, 0.08276f, 0.08276f, 1.00f)
	, new Vector4(0.08511f, 0.08511f, 0.08511f, 1.00f)
	, new Vector4(0.13531f, 0.13531f, 0.13531f, 1.00f)
	, new Vector4(0.11691f, 0.11691f, 0.11691f, 1.00f)
	, new Vector4(0.1007f, 0.1007f, 0.1007f, 1.00f)
	, new Vector4(0.09297f, 0.09297f, 0.09297f, 1.00f)
	, new Vector4(0.13531f, 0.13531f, 0.13531f, 1.00f)
	, new Vector4(0.12256f, 0.12256f, 0.12256f, 1.00f)
	, new Vector4(0.08223f, 0.08223f, 0.08223f, 1.00f)
	, new Vector4(0.13531f, 0.13531f, 0.13531f, 1.00f)
	, new Vector4(0.13531f, 0.13531f, 0.13531f, 1.00f)
	, new Vector4(0.09086f, 0.09086f, 0.09086f, 1.00f)
	, new Vector4(0.10423f, 0.10423f, 0.10423f, 1.00f)
};

// vs_cbuf0[21] = new Vector4(1.0935697E-14f, 8E-45f, 8.2508E-41f, 0f);
// vs_cbuf8[0] = new Vector4(-0.7425708f, 1.493044E-08f, 0.6697676f, 1075.086f);
// vs_cbuf8[1] = new Vector4(0.339885f, 0.8616711f, 0.3768303f, 1743.908f);
// vs_cbuf8[2] = new Vector4(-0.57711935f, 0.5074672f, -0.6398518f, -3681.8398f);
// vs_cbuf8[3] = new Vector4(0f, 0f, 0f, 1.00f);
// vs_cbuf8[4] = new Vector4(1.206285f, 0f, 0f, 0f);
// vs_cbuf8[5] = new Vector4(0f, 2.144507f, 0f, 0f);
// vs_cbuf8[6] = new Vector4(0f, 0f, -1.000008f, -0.2000008f);
// vs_cbuf8[7] = new Vector4(0f, 0f, -1f, 0f);
// vs_cbuf8[29] = new Vector4(-1919.2622f, 365.7373f, -3733.0469f, 0f);
// vs_cbuf8[30] = new Vector4(0.10f, 25000.00f, 2500.00f, 24999.90f);
// vs_cbuf9[0] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[1] = new Vector4(17441310000000000000000000000.00f, 1.42E-43f, 0f, 0f);
// vs_cbuf9[2] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[3] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[4] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[5] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[6] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[7] = new Vector4(1E-45f, 0f, 4.19717E-40f, 0f);
// vs_cbuf9[8] = new Vector4(0f, 1E-45f, 0f, 1E-45f);
// vs_cbuf9[9] = new Vector4(1E-45f, 0f, 0f, 0f);
// vs_cbuf9[10] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[11] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[12] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[13] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[14] = new Vector4(0f, -1f, 0f, 0f);
// vs_cbuf9[15] = new Vector4(1.00f, 0f, 0f, 0f);
// vs_cbuf9[16] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[17] = new Vector4(1.00f, 1.00f, 20.00f, 20.00f);
// vs_cbuf9[18] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[19] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[20] = new Vector4(2.00f, 1.00f, 1.00f, 1.00f);
// vs_cbuf9[21] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[22] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[23] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[24] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[25] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[26] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[27] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[28] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[29] = new Vector4(2.00f, 1.00f, 1.00f, 1.00f);
// vs_cbuf9[30] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[31] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[32] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[33] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[34] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[35] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[36] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[37] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[38] = new Vector4(2.00f, 1.00f, 1.00f, 1.00f);
// vs_cbuf9[39] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[40] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[41] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[42] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[43] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[44] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[45] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[46] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[47] = new Vector4(4.00f, 1.00f, 1.00f, 1.00f);
// vs_cbuf9[48] = new Vector4(0f, 1E-45f, 3E-45f, 4E-45f);
// vs_cbuf9[49] = new Vector4(6E-45f, 7E-45f, 8E-45f, 1E-44f);
// vs_cbuf9[50] = new Vector4(1.1E-44f, 1.3E-44f, 1.4E-44f, 1.5E-44f);
// vs_cbuf9[51] = new Vector4(1.7E-44f, 1.8E-44f, 2E-44f, 2.1E-44f);
// vs_cbuf9[52] = new Vector4(2.2E-44f, 2.4E-44f, 2.5E-44f, 2.7E-44f);
// vs_cbuf9[53] = new Vector4(2.8E-44f, 3E-44f, 3.1E-44f, 3.2E-44f);
// vs_cbuf9[54] = new Vector4(3.4E-44f, 3.5E-44f, 3.6E-44f, 3.8E-44f);
// vs_cbuf9[55] = new Vector4(3.9E-44f, 4E-44f, 4.2E-44f, 4.3E-44f);
// vs_cbuf9[56] = new Vector4(4.00f, 1.00f, 1.00f, 1.00f);
// vs_cbuf9[57] = new Vector4(0f, 1E-45f, 3E-45f, 4E-45f);
// vs_cbuf9[58] = new Vector4(6E-45f, 7E-45f, 8E-45f, 1E-44f);
// vs_cbuf9[59] = new Vector4(1.1E-44f, 1.3E-44f, 1.4E-44f, 1.5E-44f);
// vs_cbuf9[60] = new Vector4(1.7E-44f, 1.8E-44f, 2E-44f, 2.1E-44f);
// vs_cbuf9[61] = new Vector4(2.2E-44f, 2.4E-44f, 2.5E-44f, 2.7E-44f);
// vs_cbuf9[62] = new Vector4(2.8E-44f, 3E-44f, 3.1E-44f, 3.2E-44f);
// vs_cbuf9[63] = new Vector4(3.4E-44f, 3.5E-44f, 3.6E-44f, 3.8E-44f);
// vs_cbuf9[64] = new Vector4(3.9E-44f, 4E-44f, 4.2E-44f, 4.3E-44f);
// vs_cbuf9[65] = new Vector4(4.00f, 1.00f, 1.00f, 1.00f);
// vs_cbuf9[66] = new Vector4(0f, 1E-45f, 3E-45f, 4E-45f);
// vs_cbuf9[67] = new Vector4(6E-45f, 7E-45f, 8E-45f, 1E-44f);
// vs_cbuf9[68] = new Vector4(1.1E-44f, 1.3E-44f, 1.4E-44f, 1.5E-44f);
// vs_cbuf9[69] = new Vector4(1.7E-44f, 1.8E-44f, 2E-44f, 2.1E-44f);
// vs_cbuf9[70] = new Vector4(2.2E-44f, 2.4E-44f, 2.5E-44f, 2.7E-44f);
// vs_cbuf9[71] = new Vector4(2.8E-44f, 3E-44f, 3.1E-44f, 3.2E-44f);
// vs_cbuf9[72] = new Vector4(3.4E-44f, 3.5E-44f, 3.6E-44f, 3.8E-44f);
// vs_cbuf9[73] = new Vector4(3.9E-44f, 4E-44f, 4.2E-44f, 4.3E-44f);
// vs_cbuf9[74] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[75] = new Vector4(1.00f, 1.00f, 0f, 0f);
// vs_cbuf9[76] = new Vector4(2.00f, 2.00f, 0f, 0f);
// vs_cbuf9[77] = new Vector4(0.0008727f, 0f, 0f, 0f);
// vs_cbuf9[78] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
// vs_cbuf9[79] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[80] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[81] = new Vector4(1.00f, 1.00f, 0f, 0f);
// vs_cbuf9[82] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[83] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
// vs_cbuf9[84] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[85] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[86] = new Vector4(1.00f, 1.00f, 0f, 0f);
// vs_cbuf9[87] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[88] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
// vs_cbuf9[89] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[90] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[91] = new Vector4(1.00f, 1.00f, 0f, 0f);
// vs_cbuf9[92] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[93] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
// vs_cbuf9[94] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[95] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[96] = new Vector4(1.00f, 1.00f, 0f, 0f);
// vs_cbuf9[97] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[98] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
// vs_cbuf9[99] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[100] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[101] = new Vector4(1.00f, 1.00f, 0f, 0f);
// vs_cbuf9[102] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[103] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
// vs_cbuf9[104] = new Vector4(0.30f, 0f, 0f, 0f);
// vs_cbuf9[105] = new Vector4(0.8475056f, 0.9126984f, 0.9111589f, 0f);
// vs_cbuf9[106] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[107] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[108] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[109] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[110] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[111] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[112] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[113] = new Vector4(1.50f, 1.00f, 1.00f, 0f);
// vs_cbuf9[114] = new Vector4(1.50f, 1.00f, 1.00f, 1.00f);
// vs_cbuf9[115] = new Vector4(1.50f, 1.00f, 1.00f, 2.00f);
// vs_cbuf9[116] = new Vector4(1.50f, 1.00f, 1.00f, 3.00f);
// vs_cbuf9[117] = new Vector4(1.50f, 1.00f, 1.00f, 4.00f);
// vs_cbuf9[118] = new Vector4(1.50f, 1.00f, 1.00f, 5.00f);
// vs_cbuf9[119] = new Vector4(1.50f, 1.00f, 1.00f, 6.00f);
// vs_cbuf9[120] = new Vector4(1.50f, 1.00f, 1.00f, 7.00f);
// vs_cbuf9[121] = new Vector4(0.4303351f, 0.4726913f, 0.484127f, 0f);
// vs_cbuf9[122] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[123] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[124] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[125] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[126] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[127] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[128] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf9[129] = new Vector4(1.00f, 1.00f, 1.00f, 0f);
// vs_cbuf9[130] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
// vs_cbuf9[131] = new Vector4(1.00f, 1.00f, 1.00f, 2.00f);
// vs_cbuf9[132] = new Vector4(1.00f, 1.00f, 1.00f, 3.00f);
// vs_cbuf9[133] = new Vector4(1.00f, 1.00f, 1.00f, 4.00f);
// vs_cbuf9[134] = new Vector4(1.00f, 1.00f, 1.00f, 5.00f);
// vs_cbuf9[135] = new Vector4(1.00f, 1.00f, 1.00f, 6.00f);
// vs_cbuf9[136] = new Vector4(1.00f, 1.00f, 1.00f, 7.00f);
// vs_cbuf9[137] = new Vector4(0f, 0.50f, 1.00f, 0f);
// vs_cbuf9[138] = new Vector4(10.00f, 30.00f, 80.00f, 100.00f);
// vs_cbuf9[139] = new Vector4(1.00f, 0f, 0f, 0f);
// vs_cbuf9[140] = new Vector4(0f, 100.00f, 0f, 0f);
// vs_cbuf9[141] = new Vector4(1.00f, 1.00f, 1.00f, 0f);
// vs_cbuf10[0] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
// vs_cbuf10[1] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
// vs_cbuf10[2] = new Vector4(999.50f, 1.00f, 1.00f, 1.00f);
// vs_cbuf10[3] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
// vs_cbuf10[4] = new Vector4(1.00f, 0f, 0f, -1134.2701f);
// vs_cbuf10[5] = new Vector4(0f, 1.00f, 0f, 35.58958f);
// vs_cbuf10[6] = new Vector4(0f, 0f, 1.00f, -3936.7583f);
// vs_cbuf10[7] = new Vector4(0f, 0f, 0f, 1.00f);
// vs_cbuf10[8] = new Vector4(1.00f, 0f, 0f, -1134.2701f);
// vs_cbuf10[9] = new Vector4(0f, 1.00f, 0f, 35.58958f);
// vs_cbuf10[10] = new Vector4(0f, 0f, 1.00f, -3936.7583f);
// vs_cbuf13[0] = new Vector4(0f, 1.00f, 1.00f, 1.00f);
// vs_cbuf15[22] = new Vector4(0.0000333f, -0.0016638935f, 0f, 0f);
// vs_cbuf15[23] = new Vector4(20.00f, 1.00f, 0.85f, -0.010725529f);
// vs_cbuf15[24] = new Vector4(0.002381f, -0.04761905f, 3.363175f, 4.00f);
// vs_cbuf15[25] = new Vector4(0.0282744f, 0.0931012f, 0.1164359f, 0.7006614f);
// vs_cbuf15[26] = new Vector4(0.0174636f, 0.1221582f, 0.2193998f, 0.20f);
// vs_cbuf15[27] = new Vector4(-0.14285715f, 0.0071429f, 250.00f, 0f);
// vs_cbuf15[28] = new Vector4(0.8802994f, -0.4663191f, -0.08728968f, 0f);
// vs_cbuf15[49] = new Vector4(0f, 0f, 0f, 0f);
// vs_cbuf15[51] = new Vector4(950.00f, 50.00f, 1.50f, 1.00f);
// vs_cbuf15[52] = new Vector4(-2116f, -3932f, 0.0025f, 0f);
// vs_cbuf15[58] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);

// vs_cbuf0[21] = , 675610624u, 6u, 58880u, 0u
// vs_cbuf8[0] = , 3208517919u, 847265891u, 1059812835u, 1149657789u
// vs_cbuf8[1] = , 1051592041u, 1063032442u, 1052831719u, 1155136785u
// vs_cbuf8[2] = , 3205742104u, 1057089886u, 3206794580u, 3311803760u
// vs_cbuf8[3] = , 0u, 0u, 0u, 1065353216u
// vs_cbuf8[4] = , 1067083659u, 0u, 0u, 0u
// vs_cbuf8[5] = , 0u, 1074347930u, 0u, 0u
// vs_cbuf8[6] = , 0u, 0u, 3212836931u, 3192704258u
// vs_cbuf8[7] = , 0u, 0u, 3212836864u, 0u
// vs_cbuf8[29] = , 3304056932u, 1136057952u, 3312013504u, 0u
// vs_cbuf8[30] = , 1036831949u, 1187205120u, 1159479296u, 1187205069u
// vs_cbuf9[0] = , 0u, 0u, 0u, 0u
// vs_cbuf9[1] = , 1851878512u, 101u, 0u, 0u
// vs_cbuf9[2] = , 0u, 0u, 0u, 0u
// vs_cbuf9[3] = , 0u, 0u, 0u, 0u
// vs_cbuf9[4] = , 0u, 0u, 0u, 0u
// vs_cbuf9[5] = , 0u, 0u, 0u, 0u
// vs_cbuf9[6] = , 0u, 0u, 0u, 0u
// vs_cbuf9[7] = , 1u, 0u, 299520u, 0u
// vs_cbuf9[8] = , 0u, 1u, 0u, 1u
// vs_cbuf9[9] = , 1u, 0u, 0u, 0u
// vs_cbuf9[10] = , 0u, 0u, 0u, 0u
// vs_cbuf9[11] = , 0u, 0u, 0u, 0u
// vs_cbuf9[12] = , 0u, 0u, 0u, 0u
// vs_cbuf9[13] = , 0u, 0u, 0u, 0u
// vs_cbuf9[14] = , 0u, 3212836864u, 0u, 0u
// vs_cbuf9[15] = , 1065353216u, 0u, 0u, 0u
// vs_cbuf9[16] = , 0u, 0u, 0u, 0u
// vs_cbuf9[17] = , 1065353216u, 1065353216u, 1101004800u, 1101004800u
// vs_cbuf9[18] = , 0u, 0u, 0u, 0u
// vs_cbuf9[19] = , 0u, 0u, 0u, 0u
// vs_cbuf9[20] = , 1073741824u, 1065353216u, 1065353216u, 1065353216u
// vs_cbuf9[21] = , 0u, 0u, 0u, 0u
// vs_cbuf9[22] = , 0u, 0u, 0u, 0u
// vs_cbuf9[23] = , 0u, 0u, 0u, 0u
// vs_cbuf9[24] = , 0u, 0u, 0u, 0u
// vs_cbuf9[25] = , 0u, 0u, 0u, 0u
// vs_cbuf9[26] = , 0u, 0u, 0u, 0u
// vs_cbuf9[27] = , 0u, 0u, 0u, 0u
// vs_cbuf9[28] = , 0u, 0u, 0u, 0u
// vs_cbuf9[29] = , 1073741824u, 1065353216u, 1065353216u, 1065353216u
// vs_cbuf9[30] = , 0u, 0u, 0u, 0u
// vs_cbuf9[31] = , 0u, 0u, 0u, 0u
// vs_cbuf9[32] = , 0u, 0u, 0u, 0u
// vs_cbuf9[33] = , 0u, 0u, 0u, 0u
// vs_cbuf9[34] = , 0u, 0u, 0u, 0u
// vs_cbuf9[35] = , 0u, 0u, 0u, 0u
// vs_cbuf9[36] = , 0u, 0u, 0u, 0u
// vs_cbuf9[37] = , 0u, 0u, 0u, 0u
// vs_cbuf9[38] = , 1073741824u, 1065353216u, 1065353216u, 1065353216u
// vs_cbuf9[39] = , 0u, 0u, 0u, 0u
// vs_cbuf9[40] = , 0u, 0u, 0u, 0u
// vs_cbuf9[41] = , 0u, 0u, 0u, 0u
// vs_cbuf9[42] = , 0u, 0u, 0u, 0u
// vs_cbuf9[43] = , 0u, 0u, 0u, 0u
// vs_cbuf9[44] = , 0u, 0u, 0u, 0u
// vs_cbuf9[45] = , 0u, 0u, 0u, 0u
// vs_cbuf9[46] = , 0u, 0u, 0u, 0u
// vs_cbuf9[47] = , 1082130432u, 1065353216u, 1065353216u, 1065353216u
// vs_cbuf9[48] = , 0u, 1u, 2u, 3u
// vs_cbuf9[49] = , 4u, 5u, 6u, 7u
// vs_cbuf9[50] = , 8u, 9u, 10u, 11u
// vs_cbuf9[51] = , 12u, 13u, 14u, 15u
// vs_cbuf9[52] = , 16u, 17u, 18u, 19u
// vs_cbuf9[53] = , 20u, 21u, 22u, 23u
// vs_cbuf9[54] = , 24u, 25u, 26u, 27u
// vs_cbuf9[55] = , 28u, 29u, 30u, 31u
// vs_cbuf9[56] = , 1082130432u, 1065353216u, 1065353216u, 1065353216u
// vs_cbuf9[57] = , 0u, 1u, 2u, 3u
// vs_cbuf9[58] = , 4u, 5u, 6u, 7u
// vs_cbuf9[59] = , 8u, 9u, 10u, 11u
// vs_cbuf9[60] = , 12u, 13u, 14u, 15u
// vs_cbuf9[61] = , 16u, 17u, 18u, 19u
// vs_cbuf9[62] = , 20u, 21u, 22u, 23u
// vs_cbuf9[63] = , 24u, 25u, 26u, 27u
// vs_cbuf9[64] = , 28u, 29u, 30u, 31u
// vs_cbuf9[65] = , 1082130432u, 1065353216u, 1065353216u, 1065353216u
// vs_cbuf9[66] = , 0u, 1u, 2u, 3u
// vs_cbuf9[67] = , 4u, 5u, 6u, 7u
// vs_cbuf9[68] = , 8u, 9u, 10u, 11u
// vs_cbuf9[69] = , 12u, 13u, 14u, 15u
// vs_cbuf9[70] = , 16u, 17u, 18u, 19u
// vs_cbuf9[71] = , 20u, 21u, 22u, 23u
// vs_cbuf9[72] = , 24u, 25u, 26u, 27u
// vs_cbuf9[73] = , 28u, 29u, 30u, 31u
// vs_cbuf9[74] = , 0u, 0u, 0u, 0u
// vs_cbuf9[75] = , 1065353216u, 1065353216u, 0u, 0u
// vs_cbuf9[76] = , 1073741824u, 1073741824u, 0u, 0u
// vs_cbuf9[77] = , 979682184u, 0u, 0u, 0u
// vs_cbuf9[78] = , 1065353216u, 1065353216u, 1065353216u, 1065353216u
// vs_cbuf9[79] = , 0u, 0u, 0u, 0u
// vs_cbuf9[80] = , 0u, 0u, 0u, 0u
// vs_cbuf9[81] = , 1065353216u, 1065353216u, 0u, 0u
// vs_cbuf9[82] = , 0u, 0u, 0u, 0u
// vs_cbuf9[83] = , 1065353216u, 1065353216u, 1065353216u, 1065353216u
// vs_cbuf9[84] = , 0u, 0u, 0u, 0u
// vs_cbuf9[85] = , 0u, 0u, 0u, 0u
// vs_cbuf9[86] = , 1065353216u, 1065353216u, 0u, 0u
// vs_cbuf9[87] = , 0u, 0u, 0u, 0u
// vs_cbuf9[88] = , 1065353216u, 1065353216u, 1065353216u, 1065353216u
// vs_cbuf9[89] = , 0u, 0u, 0u, 0u
// vs_cbuf9[90] = , 0u, 0u, 0u, 0u
// vs_cbuf9[91] = , 1065353216u, 1065353216u, 0u, 0u
// vs_cbuf9[92] = , 0u, 0u, 0u, 0u
// vs_cbuf9[93] = , 1065353216u, 1065353216u, 1065353216u, 1065353216u
// vs_cbuf9[94] = , 0u, 0u, 0u, 0u
// vs_cbuf9[95] = , 0u, 0u, 0u, 0u
// vs_cbuf9[96] = , 1065353216u, 1065353216u, 0u, 0u
// vs_cbuf9[97] = , 0u, 0u, 0u, 0u
// vs_cbuf9[98] = , 1065353216u, 1065353216u, 1065353216u, 1065353216u
// vs_cbuf9[99] = , 0u, 0u, 0u, 0u
// vs_cbuf9[100] = , 0u, 0u, 0u, 0u
// vs_cbuf9[101] = , 1065353216u, 1065353216u, 0u, 0u
// vs_cbuf9[102] = , 0u, 0u, 0u, 0u
// vs_cbuf9[103] = , 1065353216u, 1065353216u, 1065353216u, 1065353216u
// vs_cbuf9[104] = , 1050253722u, 0u, 0u, 0u
// vs_cbuf9[105] = , 1062794785u, 1063888538u, 1063862710u, 0u
// vs_cbuf9[106] = , 0u, 0u, 0u, 0u
// vs_cbuf9[107] = , 0u, 0u, 0u, 0u
// vs_cbuf9[108] = , 0u, 0u, 0u, 0u
// vs_cbuf9[109] = , 0u, 0u, 0u, 0u
// vs_cbuf9[110] = , 0u, 0u, 0u, 0u
// vs_cbuf9[111] = , 0u, 0u, 0u, 0u
// vs_cbuf9[112] = , 0u, 0u, 0u, 0u
// vs_cbuf9[113] = , 1069547520u, 1065353216u, 1065353216u, 0u
// vs_cbuf9[114] = , 1069547520u, 1065353216u, 1065353216u, 1065353216u
// vs_cbuf9[115] = , 1069547520u, 1065353216u, 1065353216u, 1073741824u
// vs_cbuf9[116] = , 1069547520u, 1065353216u, 1065353216u, 1077936128u
// vs_cbuf9[117] = , 1069547520u, 1065353216u, 1065353216u, 1082130432u
// vs_cbuf9[118] = , 1069547520u, 1065353216u, 1065353216u, 1084227584u
// vs_cbuf9[119] = , 1069547520u, 1065353216u, 1065353216u, 1086324736u
// vs_cbuf9[120] = , 1069547520u, 1065353216u, 1065353216u, 1088421888u
// vs_cbuf9[121] = , 1054627042u, 1056048280u, 1056431999u, 0u
// vs_cbuf9[122] = , 0u, 0u, 0u, 0u
// vs_cbuf9[123] = , 0u, 0u, 0u, 0u
// vs_cbuf9[124] = , 0u, 0u, 0u, 0u
// vs_cbuf9[125] = , 0u, 0u, 0u, 0u
// vs_cbuf9[126] = , 0u, 0u, 0u, 0u
// vs_cbuf9[127] = , 0u, 0u, 0u, 0u
// vs_cbuf9[128] = , 0u, 0u, 0u, 0u
// vs_cbuf9[129] = , 1065353216u, 1065353216u, 1065353216u, 0u
// vs_cbuf9[130] = , 1065353216u, 1065353216u, 1065353216u, 1065353216u
// vs_cbuf9[131] = , 1065353216u, 1065353216u, 1065353216u, 1073741824u
// vs_cbuf9[132] = , 1065353216u, 1065353216u, 1065353216u, 1077936128u
// vs_cbuf9[133] = , 1065353216u, 1065353216u, 1065353216u, 1082130432u
// vs_cbuf9[134] = , 1065353216u, 1065353216u, 1065353216u, 1084227584u
// vs_cbuf9[135] = , 1065353216u, 1065353216u, 1065353216u, 1086324736u
// vs_cbuf9[136] = , 1065353216u, 1065353216u, 1065353216u, 1088421888u
// vs_cbuf9[137] = , 0u, 1056964608u, 1065353216u, 0u
// vs_cbuf9[138] = , 1092616192u, 1106247680u, 1117782016u, 1120403456u
// vs_cbuf9[139] = , 1065353216u, 0u, 0u, 0u
// vs_cbuf9[140] = , 0u, 1120403456u, 0u, 0u
// vs_cbuf9[141] = , 1065353216u, 1065353216u, 1065353216u, 0u
// vs_cbuf10[0] = , 1065353216u, 1065353216u, 1065353216u, 1065353216u
// vs_cbuf10[1] = , 1065353216u, 1065353216u, 1065353216u, 1065353216u
// vs_cbuf10[2] = , 1148837888u, 1065353216u, 1065353216u, 1065353216u
// vs_cbuf10[3] = , 1065353216u, 1065353216u, 1065353216u, 1065353216u
// vs_cbuf10[4] = , 1065353216u, 0u, 0u, 3297626277u
// vs_cbuf10[5] = , 0u, 1065353216u, 0u, 1108237243u
// vs_cbuf10[6] = , 0u, 0u, 1065353216u, 3312847906u
// vs_cbuf10[7] = , 0u, 0u, 0u, 1065353216u
// vs_cbuf10[8] = , 1065353216u, 0u, 0u, 3297626277u
// vs_cbuf10[9] = , 0u, 1065353216u, 0u, 1108237243u
// vs_cbuf10[10] = , 0u, 0u, 1065353216u, 3312847906u
// vs_cbuf13[0] = , 0u, 1065353216u, 1065353216u, 1065353216u
// vs_cbuf15[22] = , 940282839u, 3134854912u, 0u, 0u
// vs_cbuf15[23] = , 1101004800u, 1065353216u, 1062836634u, 3157244449u
// vs_cbuf15[24] = , 991693249u, 3175287857u, 1079459393u, 1082130432u
// vs_cbuf15[25] = , 1021812663u, 1035906006u, 1039037935u, 1060331147u
// vs_cbuf15[26] = , 1016008658u, 1039805978u, 1046522458u, 1045220557u
// vs_cbuf15[27] = , 3188869413u, 1005194913u, 1132068864u, 0u
// vs_cbuf15[28] = , 1063344973u, 3203318113u, 3182609647u, 0u
// vs_cbuf15[49] = , 0u, 0u, 0u, 0u
// vs_cbuf15[51] = , 1148026880u, 1112014848u, 1069547520u, 1065353216u
// vs_cbuf15[52] = , 3305390080u, 3312828416u, 992204554u, 0u
// vs_cbuf15[58] = , 1065353216u, 1065353216u, 1065353216u, 1065353216u

// 1065353216 = 1.00f;
// vs_cbuf0[21] = vec4(1.0935697E-14, 8E-45, 8.2508E-41, 0);
// vs_cbuf8[0] = vec4(-0.7425708, 1.493044E-08, 0.6697676, 1075.086);
// vs_cbuf8[1] = vec4(0.339885, 0.8616711, 0.3768303, 1743.908);
// vs_cbuf8[2] = vec4(-0.57711935, 0.5074672, -0.6398518, -3681.8398);
// vs_cbuf8[3] = vec4(0, 0, 0, 1.00);
// vs_cbuf8[4] = vec4(1.206285, 0, 0, 0);
// vs_cbuf8[5] = vec4(0, 2.144507, 0, 0);
// vs_cbuf8[6] = vec4(0, 0, -1.000008, -0.2000008);
// vs_cbuf8[7] = vec4(0, 0, -1, 0);
// vs_cbuf8[29] = vec4(-1919.2622, 365.7373, -3733.0469, 0);
// vs_cbuf8[30] = vec4(0.10, 25000.00, 2500.00, 24999.90);
// vs_cbuf9[0] = vec4(0, 0, 0, 0);
// vs_cbuf9[1] = vec4(17441310000000000000000000000.00, 1.42E-43, 0, 0);
// vs_cbuf9[2] = vec4(0, 0, 0, 0);
// vs_cbuf9[3] = vec4(0, 0, 0, 0);
// vs_cbuf9[4] = vec4(0, 0, 0, 0);
// vs_cbuf9[5] = vec4(0, 0, 0, 0);
// vs_cbuf9[6] = vec4(0, 0, 0, 0);
// vs_cbuf9[7] = vec4(1E-45, 0, 4.19717E-40, 0);
// vs_cbuf9[8] = vec4(0, 1E-45, 0, 1E-45);
// vs_cbuf9[9] = vec4(1E-45, 0, 0, 0);
// vs_cbuf9[10] = vec4(0, 0, 0, 0);
// vs_cbuf9[11] = vec4(0, 0, 0, 0);
// vs_cbuf9[12] = vec4(0, 0, 0, 0);
// vs_cbuf9[13] = vec4(0, 0, 0, 0);
// vs_cbuf9[14] = vec4(0, -1, 0, 0);
// vs_cbuf9[15] = vec4(1.00, 0, 0, 0);
// vs_cbuf9[16] = vec4(0, 0, 0, 0);
// vs_cbuf9[17] = vec4(1.00, 1.00, 20.00, 20.00);
// vs_cbuf9[18] = vec4(0, 0, 0, 0);
// vs_cbuf9[19] = vec4(0, 0, 0, 0);
// vs_cbuf9[20] = vec4(2.00, 1.00, 1.00, 1.00);
// vs_cbuf9[21] = vec4(0, 0, 0, 0);
// vs_cbuf9[22] = vec4(0, 0, 0, 0);
// vs_cbuf9[23] = vec4(0, 0, 0, 0);
// vs_cbuf9[24] = vec4(0, 0, 0, 0);
// vs_cbuf9[25] = vec4(0, 0, 0, 0);
// vs_cbuf9[26] = vec4(0, 0, 0, 0);
// vs_cbuf9[27] = vec4(0, 0, 0, 0);
// vs_cbuf9[28] = vec4(0, 0, 0, 0);
// vs_cbuf9[29] = vec4(2.00, 1.00, 1.00, 1.00);
// vs_cbuf9[30] = vec4(0, 0, 0, 0);
// vs_cbuf9[31] = vec4(0, 0, 0, 0);
// vs_cbuf9[32] = vec4(0, 0, 0, 0);
// vs_cbuf9[33] = vec4(0, 0, 0, 0);
// vs_cbuf9[34] = vec4(0, 0, 0, 0);
// vs_cbuf9[35] = vec4(0, 0, 0, 0);
// vs_cbuf9[36] = vec4(0, 0, 0, 0);
// vs_cbuf9[37] = vec4(0, 0, 0, 0);
// vs_cbuf9[38] = vec4(2.00, 1.00, 1.00, 1.00);
// vs_cbuf9[39] = vec4(0, 0, 0, 0);
// vs_cbuf9[40] = vec4(0, 0, 0, 0);
// vs_cbuf9[41] = vec4(0, 0, 0, 0);
// vs_cbuf9[42] = vec4(0, 0, 0, 0);
// vs_cbuf9[43] = vec4(0, 0, 0, 0);
// vs_cbuf9[44] = vec4(0, 0, 0, 0);
// vs_cbuf9[45] = vec4(0, 0, 0, 0);
// vs_cbuf9[46] = vec4(0, 0, 0, 0);
// vs_cbuf9[47] = vec4(4.00, 1.00, 1.00, 1.00);
// vs_cbuf9[48] = vec4(0, 1E-45, 3E-45, 4E-45);
// vs_cbuf9[49] = vec4(6E-45, 7E-45, 8E-45, 1E-44);
// vs_cbuf9[50] = vec4(1.1E-44, 1.3E-44, 1.4E-44, 1.5E-44);
// vs_cbuf9[51] = vec4(1.7E-44, 1.8E-44, 2E-44, 2.1E-44);
// vs_cbuf9[52] = vec4(2.2E-44, 2.4E-44, 2.5E-44, 2.7E-44);
// vs_cbuf9[53] = vec4(2.8E-44, 3E-44, 3.1E-44, 3.2E-44);
// vs_cbuf9[54] = vec4(3.4E-44, 3.5E-44, 3.6E-44, 3.8E-44);
// vs_cbuf9[55] = vec4(3.9E-44, 4E-44, 4.2E-44, 4.3E-44);
// vs_cbuf9[56] = vec4(4.00, 1.00, 1.00, 1.00);
// vs_cbuf9[57] = vec4(0, 1E-45, 3E-45, 4E-45);
// vs_cbuf9[58] = vec4(6E-45, 7E-45, 8E-45, 1E-44);
// vs_cbuf9[59] = vec4(1.1E-44, 1.3E-44, 1.4E-44, 1.5E-44);
// vs_cbuf9[60] = vec4(1.7E-44, 1.8E-44, 2E-44, 2.1E-44);
// vs_cbuf9[61] = vec4(2.2E-44, 2.4E-44, 2.5E-44, 2.7E-44);
// vs_cbuf9[62] = vec4(2.8E-44, 3E-44, 3.1E-44, 3.2E-44);
// vs_cbuf9[63] = vec4(3.4E-44, 3.5E-44, 3.6E-44, 3.8E-44);
// vs_cbuf9[64] = vec4(3.9E-44, 4E-44, 4.2E-44, 4.3E-44);
// vs_cbuf9[65] = vec4(4.00, 1.00, 1.00, 1.00);
// vs_cbuf9[66] = vec4(0, 1E-45, 3E-45, 4E-45);
// vs_cbuf9[67] = vec4(6E-45, 7E-45, 8E-45, 1E-44);
// vs_cbuf9[68] = vec4(1.1E-44, 1.3E-44, 1.4E-44, 1.5E-44);
// vs_cbuf9[69] = vec4(1.7E-44, 1.8E-44, 2E-44, 2.1E-44);
// vs_cbuf9[70] = vec4(2.2E-44, 2.4E-44, 2.5E-44, 2.7E-44);
// vs_cbuf9[71] = vec4(2.8E-44, 3E-44, 3.1E-44, 3.2E-44);
// vs_cbuf9[72] = vec4(3.4E-44, 3.5E-44, 3.6E-44, 3.8E-44);
// vs_cbuf9[73] = vec4(3.9E-44, 4E-44, 4.2E-44, 4.3E-44);
// vs_cbuf9[74] = vec4(0, 0, 0, 0);
// vs_cbuf9[75] = vec4(1.00, 1.00, 0, 0);
// vs_cbuf9[76] = vec4(2.00, 2.00, 0, 0);
// vs_cbuf9[77] = vec4(0.0008727, 0, 0, 0);
// vs_cbuf9[78] = vec4(1.00, 1.00, 1.00, 1.00);
// vs_cbuf9[79] = vec4(0, 0, 0, 0);
// vs_cbuf9[80] = vec4(0, 0, 0, 0);
// vs_cbuf9[81] = vec4(1.00, 1.00, 0, 0);
// vs_cbuf9[82] = vec4(0, 0, 0, 0);
// vs_cbuf9[83] = vec4(1.00, 1.00, 1.00, 1.00);
// vs_cbuf9[84] = vec4(0, 0, 0, 0);
// vs_cbuf9[85] = vec4(0, 0, 0, 0);
// vs_cbuf9[86] = vec4(1.00, 1.00, 0, 0);
// vs_cbuf9[87] = vec4(0, 0, 0, 0);
// vs_cbuf9[88] = vec4(1.00, 1.00, 1.00, 1.00);
// vs_cbuf9[89] = vec4(0, 0, 0, 0);
// vs_cbuf9[90] = vec4(0, 0, 0, 0);
// vs_cbuf9[91] = vec4(1.00, 1.00, 0, 0);
// vs_cbuf9[92] = vec4(0, 0, 0, 0);
// vs_cbuf9[93] = vec4(1.00, 1.00, 1.00, 1.00);
// vs_cbuf9[94] = vec4(0, 0, 0, 0);
// vs_cbuf9[95] = vec4(0, 0, 0, 0);
// vs_cbuf9[96] = vec4(1.00, 1.00, 0, 0);
// vs_cbuf9[97] = vec4(0, 0, 0, 0);
// vs_cbuf9[98] = vec4(1.00, 1.00, 1.00, 1.00);
// vs_cbuf9[99] = vec4(0, 0, 0, 0);
// vs_cbuf9[100] = vec4(0, 0, 0, 0);
// vs_cbuf9[101] = vec4(1.00, 1.00, 0, 0);
// vs_cbuf9[102] = vec4(0, 0, 0, 0);
// vs_cbuf9[103] = vec4(1.00, 1.00, 1.00, 1.00);
// vs_cbuf9[104] = vec4(0.30, 0, 0, 0);
// vs_cbuf9[105] = vec4(0.8475056, 0.9126984, 0.9111589, 0);
// vs_cbuf9[106] = vec4(0, 0, 0, 0);
// vs_cbuf9[107] = vec4(0, 0, 0, 0);
// vs_cbuf9[108] = vec4(0, 0, 0, 0);
// vs_cbuf9[109] = vec4(0, 0, 0, 0);
// vs_cbuf9[110] = vec4(0, 0, 0, 0);
// vs_cbuf9[111] = vec4(0, 0, 0, 0);
// vs_cbuf9[112] = vec4(0, 0, 0, 0);
// vs_cbuf9[113] = vec4(1.50, 1.00, 1.00, 0);
// vs_cbuf9[114] = vec4(1.50, 1.00, 1.00, 1.00);
// vs_cbuf9[115] = vec4(1.50, 1.00, 1.00, 2.00);
// vs_cbuf9[116] = vec4(1.50, 1.00, 1.00, 3.00);
// vs_cbuf9[117] = vec4(1.50, 1.00, 1.00, 4.00);
// vs_cbuf9[118] = vec4(1.50, 1.00, 1.00, 5.00);
// vs_cbuf9[119] = vec4(1.50, 1.00, 1.00, 6.00);
// vs_cbuf9[120] = vec4(1.50, 1.00, 1.00, 7.00);
// vs_cbuf9[121] = vec4(0.4303351, 0.4726913, 0.484127, 0);
// vs_cbuf9[122] = vec4(0, 0, 0, 0);
// vs_cbuf9[123] = vec4(0, 0, 0, 0);
// vs_cbuf9[124] = vec4(0, 0, 0, 0);
// vs_cbuf9[125] = vec4(0, 0, 0, 0);
// vs_cbuf9[126] = vec4(0, 0, 0, 0);
// vs_cbuf9[127] = vec4(0, 0, 0, 0);
// vs_cbuf9[128] = vec4(0, 0, 0, 0);
// vs_cbuf9[129] = vec4(1.00, 1.00, 1.00, 0);
// vs_cbuf9[130] = vec4(1.00, 1.00, 1.00, 1.00);
// vs_cbuf9[131] = vec4(1.00, 1.00, 1.00, 2.00);
// vs_cbuf9[132] = vec4(1.00, 1.00, 1.00, 3.00);
// vs_cbuf9[133] = vec4(1.00, 1.00, 1.00, 4.00);
// vs_cbuf9[134] = vec4(1.00, 1.00, 1.00, 5.00);
// vs_cbuf9[135] = vec4(1.00, 1.00, 1.00, 6.00);
// vs_cbuf9[136] = vec4(1.00, 1.00, 1.00, 7.00);
// vs_cbuf9[137] = vec4(0, 0.50, 1.00, 0);
// vs_cbuf9[138] = vec4(10.00, 30.00, 80.00, 100.00);
// vs_cbuf9[139] = vec4(1.00, 0, 0, 0);
// vs_cbuf9[140] = vec4(0, 100.00, 0, 0);
// vs_cbuf9[141] = vec4(1.00, 1.00, 1.00, 0);
// vs_cbuf10[0] = vec4(1.00, 1.00, 1.00, 1.00);
// vs_cbuf10[1] = vec4(1.00, 1.00, 1.00, 1.00);
// vs_cbuf10[2] = vec4(999.50, 1.00, 1.00, 1.00);
// vs_cbuf10[3] = vec4(1.00, 1.00, 1.00, 1.00);
// vs_cbuf10[4] = vec4(1.00, 0, 0, -1134.2701);
// vs_cbuf10[5] = vec4(0, 1.00, 0, 35.58958);
// vs_cbuf10[6] = vec4(0, 0, 1.00, -3936.7583);
// vs_cbuf10[7] = vec4(0, 0, 0, 1.00);
// vs_cbuf10[8] = vec4(1.00, 0, 0, -1134.2701);
// vs_cbuf10[9] = vec4(0, 1.00, 0, 35.58958);
// vs_cbuf10[10] = vec4(0, 0, 1.00, -3936.7583);
// vs_cbuf13[0] = vec4(0, 1.00, 1.00, 1.00);
// vs_cbuf15[22] = vec4(0.0000333, -0.0016638935, 0, 0);
// vs_cbuf15[23] = vec4(20.00, 1.00, 0.85, -0.010725529);
// vs_cbuf15[24] = vec4(0.002381, -0.04761905, 3.363175, 4.00);
// vs_cbuf15[25] = vec4(0.0282744, 0.0931012, 0.1164359, 0.7006614);
// vs_cbuf15[26] = vec4(0.0174636, 0.1221582, 0.2193998, 0.20);
// vs_cbuf15[27] = vec4(-0.14285715, 0.0071429, 250.00, 0);
// vs_cbuf15[28] = vec4(0.8802994, -0.4663191, -0.08728968, 0);
// vs_cbuf15[49] = vec4(0, 0, 0, 0);
// vs_cbuf15[51] = vec4(950.00, 50.00, 1.50, 1.00);
// vs_cbuf15[52] = vec4(-2116, -3932, 0.0025, 0);
// vs_cbuf15[58] = vec4(1.00, 1.00, 1.00, 1.00);

[NonSerialized]
private Vector4[] in_attr4_array = new[] {
	new Vector4(0.0f, 0.0f, 0.0f, 268435456.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(720.0f, 720.0f, 720.0f, 1.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.5484035015106201f, 0.9605059027671814f, 0.5883466005325317f, 0.4289205074310303f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(-14.717313766479492f, 0.0f, -136.69692993164062f, 300.0f)
	, new Vector4(69.15032196044922f, 0.0f, -51.24028396606445f, 300.0f)
	, new Vector4(61.13825988769531f, 0.0f, 147.86451721191406f, 300.0f)
	, new Vector4(-54.50740432739258f, 0.0f, -159.4245147705078f, 300.0f)
	, new Vector4(7.966396331787109f, 0.0f, 48.79098892211914f, 300.0f)
	, new Vector4(108.82708740234375f, 0.0f, 44.278526306152344f, 300.0f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 912.5f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 988.5f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 608.5f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 684.5f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 760.5f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 836.5f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 0.0f)
	, new Vector4(281.89825439453125f, 281.89825439453125f, 281.89825439453125f, 1.0f)
	, new Vector4(348.92987060546875f, 348.92987060546875f, 348.92987060546875f, 1.0f)
	, new Vector4(349.5003662109375f, 349.5003662109375f, 349.5003662109375f, 1.0f)
	, new Vector4(291.8169250488281f, 291.8169250488281f, 291.8169250488281f, 1.0f)
	, new Vector4(285.5382080078125f, 285.5382080078125f, 285.5382080078125f, 1.0f)
	, new Vector4(342.3774108886719f, 342.3774108886719f, 342.3774108886719f, 1.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 1.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 2.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 3.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 4.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 5.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.41122713685035706f, 0.24876904487609863f, 0.6435101628303528f, 0.7812744975090027f)
	, new Vector4(0.1433422863483429f, 0.5305896997451782f, 0.16866151988506317f, 0.32712608575820923f)
	, new Vector4(0.40681448578834534f, 0.3923303782939911f, 0.6349537968635559f, 0.13464239239692688f)
	, new Vector4(0.9296658635139465f, 0.07352636009454727f, 0.016763266175985336f, 0.7534669041633606f)
	, new Vector4(0.7990067601203918f, 0.15740340948104858f, 0.43572452664375305f, 0.8198761343955994f)
	, new Vector4(0.8193761110305786f, 0.2992602586746216f, 0.037938084453344345f, 0.10195884108543396f)
	, new Vector4(1.0f, 0.0f, 0.0f, -1134.2701416015625f)
	, new Vector4(1.0f, 0.0f, 0.0f, -1134.2701416015625f)
	, new Vector4(1.0f, 0.0f, 0.0f, -1134.2701416015625f)
	, new Vector4(1.0f, 0.0f, 0.0f, -1134.2701416015625f)
	, new Vector4(1.0f, 0.0f, 0.0f, -1134.2701416015625f)
	, new Vector4(1.0f, 0.0f, 0.0f, -1134.2701416015625f)
	, new Vector4(0.0f, 1.0f, 0.0f, 55.58958053588867f)
	, new Vector4(0.0f, 1.0f, 0.0f, 55.58958053588867f)
	, new Vector4(0.0f, 1.0f, 0.0f, 55.58958053588867f)
	, new Vector4(0.0f, 1.0f, 0.0f, 55.58958053588867f)
	, new Vector4(0.0f, 1.0f, 0.0f, 55.58958053588867f)
	, new Vector4(0.0f, 1.0f, 0.0f, 55.58958053588867f)
	, new Vector4(0.0f, 0.0f, 1.0f, -3936.75830078125f)
	, new Vector4(0.0f, 0.0f, 1.0f, -3936.75830078125f)
	, new Vector4(0.0f, 0.0f, 1.0f, -3936.75830078125f)
	, new Vector4(0.0f, 0.0f, 1.0f, -3936.75830078125f)
	, new Vector4(0.0f, 0.0f, 1.0f, -3936.75830078125f)
	, new Vector4(0.0f, 0.0f, 1.0f, -3936.75830078125f)
	, new Vector4(0.0f, 0.0f, 0.0f, 268435456.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(720.0f, 720.0f, 720.0f, 1.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
};
[NonSerialized]
private Vector4[] in_attr5_array = new[] {
	new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(720.0f, 720.0f, 720.0f, 1.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.5484035015106201f, 0.9605059027671814f, 0.5883466005325317f, 0.4289205074310303f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(-14.717313766479492f, 0.0f, -136.69692993164062f, 300.0f)
	, new Vector4(69.15032196044922f, 0.0f, -51.24028396606445f, 300.0f)
	, new Vector4(61.13825988769531f, 0.0f, 147.86451721191406f, 300.0f)
	, new Vector4(-54.50740432739258f, 0.0f, -159.4245147705078f, 300.0f)
	, new Vector4(7.966396331787109f, 0.0f, 48.79098892211914f, 300.0f)
	, new Vector4(108.82708740234375f, 0.0f, 44.278526306152344f, 300.0f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 912.5f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 988.5f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 608.5f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 684.5f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 760.5f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 836.5f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 0.0f)
	, new Vector4(281.89825439453125f, 281.89825439453125f, 281.89825439453125f, 1.0f)
	, new Vector4(348.92987060546875f, 348.92987060546875f, 348.92987060546875f, 1.0f)
	, new Vector4(349.5003662109375f, 349.5003662109375f, 349.5003662109375f, 1.0f)
	, new Vector4(291.8169250488281f, 291.8169250488281f, 291.8169250488281f, 1.0f)
	, new Vector4(285.5382080078125f, 285.5382080078125f, 285.5382080078125f, 1.0f)
	, new Vector4(342.3774108886719f, 342.3774108886719f, 342.3774108886719f, 1.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 1.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 2.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 3.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 4.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 5.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.41122713685035706f, 0.24876904487609863f, 0.6435101628303528f, 0.7812744975090027f)
	, new Vector4(0.1433422863483429f, 0.5305896997451782f, 0.16866151988506317f, 0.32712608575820923f)
	, new Vector4(0.40681448578834534f, 0.3923303782939911f, 0.6349537968635559f, 0.13464239239692688f)
	, new Vector4(0.9296658635139465f, 0.07352636009454727f, 0.016763266175985336f, 0.7534669041633606f)
	, new Vector4(0.7990067601203918f, 0.15740340948104858f, 0.43572452664375305f, 0.8198761343955994f)
	, new Vector4(0.8193761110305786f, 0.2992602586746216f, 0.037938084453344345f, 0.10195884108543396f)
	, new Vector4(1.0f, 0.0f, 0.0f, -1134.2701416015625f)
	, new Vector4(1.0f, 0.0f, 0.0f, -1134.2701416015625f)
	, new Vector4(1.0f, 0.0f, 0.0f, -1134.2701416015625f)
	, new Vector4(1.0f, 0.0f, 0.0f, -1134.2701416015625f)
	, new Vector4(1.0f, 0.0f, 0.0f, -1134.2701416015625f)
	, new Vector4(1.0f, 0.0f, 0.0f, -1134.2701416015625f)
	, new Vector4(0.0f, 1.0f, 0.0f, 55.58958053588867f)
	, new Vector4(0.0f, 1.0f, 0.0f, 55.58958053588867f)
	, new Vector4(0.0f, 1.0f, 0.0f, 55.58958053588867f)
	, new Vector4(0.0f, 1.0f, 0.0f, 55.58958053588867f)
	, new Vector4(0.0f, 1.0f, 0.0f, 55.58958053588867f)
	, new Vector4(0.0f, 1.0f, 0.0f, 55.58958053588867f)
	, new Vector4(0.0f, 0.0f, 1.0f, -3936.75830078125f)
	, new Vector4(0.0f, 0.0f, 1.0f, -3936.75830078125f)
	, new Vector4(0.0f, 0.0f, 1.0f, -3936.75830078125f)
	, new Vector4(0.0f, 0.0f, 1.0f, -3936.75830078125f)
	, new Vector4(0.0f, 0.0f, 1.0f, -3936.75830078125f)
	, new Vector4(0.0f, 0.0f, 1.0f, -3936.75830078125f)
	, new Vector4(0.0f, 0.0f, 0.0f, 268435456.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(720.0f, 720.0f, 720.0f, 1.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
};
[NonSerialized]
private Vector4[] in_attr6_array = new[] {
	new Vector4(720.0f, 720.0f, 720.0f, 1.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.5484035015106201f, 0.9605059027671814f, 0.5883466005325317f, 0.4289205074310303f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(-14.717313766479492f, 0.0f, -136.69692993164062f, 300.0f)
	, new Vector4(69.15032196044922f, 0.0f, -51.24028396606445f, 300.0f)
	, new Vector4(61.13825988769531f, 0.0f, 147.86451721191406f, 300.0f)
	, new Vector4(-54.50740432739258f, 0.0f, -159.4245147705078f, 300.0f)
	, new Vector4(7.966396331787109f, 0.0f, 48.79098892211914f, 300.0f)
	, new Vector4(108.82708740234375f, 0.0f, 44.278526306152344f, 300.0f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 912.5f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 988.5f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 608.5f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 684.5f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 760.5f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 836.5f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 0.0f)
	, new Vector4(281.89825439453125f, 281.89825439453125f, 281.89825439453125f, 1.0f)
	, new Vector4(348.92987060546875f, 348.92987060546875f, 348.92987060546875f, 1.0f)
	, new Vector4(349.5003662109375f, 349.5003662109375f, 349.5003662109375f, 1.0f)
	, new Vector4(291.8169250488281f, 291.8169250488281f, 291.8169250488281f, 1.0f)
	, new Vector4(285.5382080078125f, 285.5382080078125f, 285.5382080078125f, 1.0f)
	, new Vector4(342.3774108886719f, 342.3774108886719f, 342.3774108886719f, 1.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 1.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 2.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 3.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 4.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 5.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.41122713685035706f, 0.24876904487609863f, 0.6435101628303528f, 0.7812744975090027f)
	, new Vector4(0.1433422863483429f, 0.5305896997451782f, 0.16866151988506317f, 0.32712608575820923f)
	, new Vector4(0.40681448578834534f, 0.3923303782939911f, 0.6349537968635559f, 0.13464239239692688f)
	, new Vector4(0.9296658635139465f, 0.07352636009454727f, 0.016763266175985336f, 0.7534669041633606f)
	, new Vector4(0.7990067601203918f, 0.15740340948104858f, 0.43572452664375305f, 0.8198761343955994f)
	, new Vector4(0.8193761110305786f, 0.2992602586746216f, 0.037938084453344345f, 0.10195884108543396f)
	, new Vector4(1.0f, 0.0f, 0.0f, -1134.2701416015625f)
	, new Vector4(1.0f, 0.0f, 0.0f, -1134.2701416015625f)
	, new Vector4(1.0f, 0.0f, 0.0f, -1134.2701416015625f)
	, new Vector4(1.0f, 0.0f, 0.0f, -1134.2701416015625f)
	, new Vector4(1.0f, 0.0f, 0.0f, -1134.2701416015625f)
	, new Vector4(1.0f, 0.0f, 0.0f, -1134.2701416015625f)
	, new Vector4(0.0f, 1.0f, 0.0f, 55.58958053588867f)
	, new Vector4(0.0f, 1.0f, 0.0f, 55.58958053588867f)
	, new Vector4(0.0f, 1.0f, 0.0f, 55.58958053588867f)
	, new Vector4(0.0f, 1.0f, 0.0f, 55.58958053588867f)
	, new Vector4(0.0f, 1.0f, 0.0f, 55.58958053588867f)
	, new Vector4(0.0f, 1.0f, 0.0f, 55.58958053588867f)
	, new Vector4(0.0f, 0.0f, 1.0f, -3936.75830078125f)
	, new Vector4(0.0f, 0.0f, 1.0f, -3936.75830078125f)
	, new Vector4(0.0f, 0.0f, 1.0f, -3936.75830078125f)
	, new Vector4(0.0f, 0.0f, 1.0f, -3936.75830078125f)
	, new Vector4(0.0f, 0.0f, 1.0f, -3936.75830078125f)
	, new Vector4(0.0f, 0.0f, 1.0f, -3936.75830078125f)
	, new Vector4(0.0f, 0.0f, 0.0f, 268435456.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(720.0f, 720.0f, 720.0f, 1.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.6953925490379333f, 0.8510918021202087f, 0.32709574699401855f, 0.7651605606079102f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
};
[NonSerialized]
private Vector4[] in_attr7_array = new[] {
	new Vector4(0.5484035015106201f, 0.9605059027671814f, 0.5883466005325317f, 0.4289205074310303f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(-14.717313766479492f, 0.0f, -136.69692993164062f, 300.0f)
	, new Vector4(69.15032196044922f, 0.0f, -51.24028396606445f, 300.0f)
	, new Vector4(61.13825988769531f, 0.0f, 147.86451721191406f, 300.0f)
	, new Vector4(-54.50740432739258f, 0.0f, -159.4245147705078f, 300.0f)
	, new Vector4(7.966396331787109f, 0.0f, 48.79098892211914f, 300.0f)
	, new Vector4(108.82708740234375f, 0.0f, 44.278526306152344f, 300.0f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 912.5f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 988.5f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 608.5f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 684.5f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 760.5f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 836.5f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 0.0f)
	, new Vector4(281.89825439453125f, 281.89825439453125f, 281.89825439453125f, 1.0f)
	, new Vector4(348.92987060546875f, 348.92987060546875f, 348.92987060546875f, 1.0f)
	, new Vector4(349.5003662109375f, 349.5003662109375f, 349.5003662109375f, 1.0f)
	, new Vector4(291.8169250488281f, 291.8169250488281f, 291.8169250488281f, 1.0f)
	, new Vector4(285.5382080078125f, 285.5382080078125f, 285.5382080078125f, 1.0f)
	, new Vector4(342.3774108886719f, 342.3774108886719f, 342.3774108886719f, 1.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 1.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 2.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 3.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 4.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 5.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.41122713685035706f, 0.24876904487609863f, 0.6435101628303528f, 0.7812744975090027f)
	, new Vector4(0.1433422863483429f, 0.5305896997451782f, 0.16866151988506317f, 0.32712608575820923f)
	, new Vector4(0.40681448578834534f, 0.3923303782939911f, 0.6349537968635559f, 0.13464239239692688f)
	, new Vector4(0.9296658635139465f, 0.07352636009454727f, 0.016763266175985336f, 0.7534669041633606f)
	, new Vector4(0.7990067601203918f, 0.15740340948104858f, 0.43572452664375305f, 0.8198761343955994f)
	, new Vector4(0.8193761110305786f, 0.2992602586746216f, 0.037938084453344345f, 0.10195884108543396f)
	, new Vector4(1.0f, 0.0f, 0.0f, -1134.2701416015625f)
	, new Vector4(1.0f, 0.0f, 0.0f, -1134.2701416015625f)
	, new Vector4(1.0f, 0.0f, 0.0f, -1134.2701416015625f)
	, new Vector4(1.0f, 0.0f, 0.0f, -1134.2701416015625f)
	, new Vector4(1.0f, 0.0f, 0.0f, -1134.2701416015625f)
	, new Vector4(1.0f, 0.0f, 0.0f, -1134.2701416015625f)
	, new Vector4(0.0f, 1.0f, 0.0f, 55.58958053588867f)
	, new Vector4(0.0f, 1.0f, 0.0f, 55.58958053588867f)
	, new Vector4(0.0f, 1.0f, 0.0f, 55.58958053588867f)
	, new Vector4(0.0f, 1.0f, 0.0f, 55.58958053588867f)
	, new Vector4(0.0f, 1.0f, 0.0f, 55.58958053588867f)
	, new Vector4(0.0f, 1.0f, 0.0f, 55.58958053588867f)
	, new Vector4(0.0f, 0.0f, 1.0f, -3936.75830078125f)
	, new Vector4(0.0f, 0.0f, 1.0f, -3936.75830078125f)
	, new Vector4(0.0f, 0.0f, 1.0f, -3936.75830078125f)
	, new Vector4(0.0f, 0.0f, 1.0f, -3936.75830078125f)
	, new Vector4(0.0f, 0.0f, 1.0f, -3936.75830078125f)
	, new Vector4(0.0f, 0.0f, 1.0f, -3936.75830078125f)
	, new Vector4(0.0f, 0.0f, 0.0f, 268435456.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(720.0f, 720.0f, 720.0f, 1.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.6953925490379333f, 0.8510918021202087f, 0.32709574699401855f, 0.7651605606079102f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(59.53733825683594f, 0.0f, 104.73887634277344f, 300.0f)
	, new Vector4(-9.847415924072266f, 0.0f, 102.50017547607422f, 300.0f)
	, new Vector4(-176.8878173828125f, 0.0f, -2.643361806869507f, 300.0f)
	, new Vector4(-8.799407005310059f, 0.0f, 183.86302185058594f, 300.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 0.0f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 76.0f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 152.0f)
	, new Vector4(0.0f, 0.009999999776482582f, 0.0f, 228.5f)
	, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)
};

out gl_PerVertex
{
	vec4 gl_Position;
};
layout(location = 0 )in vec4 in_attr0;
layout(location = 1 )in vec4 in_attr1;
layout(location = 2 )in vec4 in_attr2;
layout(location = 3 )in vec4 in_attr3;
layout(location = 4 )in vec4 in_attr4;
layout(location = 5 )in vec4 in_attr5;
layout(location = 6 )in vec4 in_attr6;
layout(location = 7 )in vec4 in_attr7;
layout(location = 0 )out vec4 out_attr0;
layout(location = 1 )out vec4 out_attr1;
layout(location = 2 )out vec4 out_attr2;
layout(location = 3 )out vec4 out_attr3;
layout(location = 4 )out vec4 out_attr4;
layout(location = 5 )out vec4 out_attr5;
layout(location = 6 )out vec4 out_attr6;
layout(location = 7 )out vec4 out_attr7;
layout(location = 8 )out vec4 out_attr8;
layout(location = 9 )out vec4 out_attr9;
layout(location = 10 )out vec4 out_attr10;
layout(std140, binding = 0 )uniform vs_cbuf_0
{
	uvec4 vs_cbuf0[4096 ];
};
layout(std140, binding = 1 )uniform vs_cbuf_8
{
	uvec4 vs_cbuf8[4096 ];
};
layout(std140, binding = 2 )uniform vs_cbuf_9
{
	uvec4 vs_cbuf9[4096 ];
};
layout(std140, binding = 3 )uniform vs_cbuf_10
{
	uvec4 vs_cbuf10[4096 ];
};
layout(std140, binding = 4 )uniform vs_cbuf_13
{
	uvec4 vs_cbuf13[4096 ];
};
layout(std140, binding = 5 )uniform vs_cbuf_15
{
	uvec4 vs_cbuf15[4096 ];
};
layout(std430, binding = 0 )buffer vs_ssbo_0
{
	uint vs_ssbo0[];
};
layout(binding = 0 )uniform sampler2D tex0;
layout(binding = 1 )uniform sampler2D tex1;
void main()
{
	in_attr0.x  = float(0.00 );	// 0
	in_attr0.y  = float(0.00 );	// 0
	in_attr0.z  = float(0.001 );	// 0.001
	in_attr0.w  = float(1.00 );	// 1.00
	in_attr1.x  = float(0.50196 );	// 0.50196
	in_attr1.y  = float(0.50196 );	// 0.50196
	in_attr2.x  = float(0.00 );	// 0
	in_attr2.y  = float(0.00 );	// 0
	in_attr2.z  = float(1.00 );	// 1.00
	in_attr2.w  = float(1.00 );	// 1.00
	in_attr3.x  = float(1.00 );	// 1.00
	in_attr3.y  = float(1.00 );	// 1.00
	in_attr3.z  = float(1.00 );	// 1.00
	in_attr3.w  = float(1.00 );	// 1.00
	in_attr4.x  = float(0.00 );	// 0
	in_attr4.y  = float(0.00 );	// 0
	in_attr4.z  = float(0.00 );	// 0
	in_attr4.w  = float(2.68435E+08 );	// 268435000.00
	in_attr5.x  = float(0.00 );	// 0
	in_attr5.y  = float(0.00 );	// 0
	in_attr5.z  = float(0.00 );	// 0
	in_attr5.w  = float(0.00 );	// 0
	in_attr6.x  = float(720.00 );	// 720.00
	in_attr6.y  = float(720.00 );	// 720.00
	in_attr6.z  = float(720.00 );	// 720.00
	in_attr6.w  = float(1.00 );	// 1.00
	in_attr7.x  = float(0.5484 );	// 0.5484
	in_attr7.y  = float(0.96051 );	// 0.96051
	in_attr7.z  = float(0.58835 );	// 0.58835
	in_attr7.w  = float(0.42892 );	// 0.42892
	gl_Position.x  = float(-867.74292 );	/* -867.7429  <=>  float(-867.74292)
<=>
float(-867.74292)
<=>
{gl_Position.x = float(-867.74292)
}
*/
	gl_Position.y  = float(-201.18993 );	/* -201.18993  <=>  float(-201.18993)
<=>
float(-201.18993)
<=>
{gl_Position.y = float(-201.18993)
}
*/
	gl_Position.z  = float(489.66693 );	/* 489.6669  <=>  float(489.66693)
<=>
float(489.66693)
<=>
{gl_Position.z = float(489.66693)
}
*/
	gl_Position.w  = float(489.86304 );	/* 489.863  <=>  float(489.86304)
<=>
float(489.86304)
<=>
{gl_Position.w = float(489.86304)
}
*/
	out_attr0.x  = float(0.25425 );	/* 0.25425  <=>  float(0.25425)
<=>
float(0.25425)
<=>
{out_attr0.x = float(0.25425)
}
*/
	out_attr0.y  = float(0.27381 );	/* 0.27381  <=>  float(0.27381)
<=>
float(0.27381)
<=>
{out_attr0.y = float(0.27381)
}
*/
	out_attr0.z  = float(0.27335 );	/* 0.27335  <=>  float(0.27335)
<=>
float(0.27335)
<=>
{out_attr0.z = float(0.27335)
}
*/
	out_attr0.w  = float(1.50 );	/* 1.50  <=>  float(1.50)
<=>
float(1.50)
<=>
{out_attr0.w = float(1.50)
}
*/
	out_attr1.x  = float(0.1291 );	/* 0.1291  <=>  float(0.1291)
<=>
float(0.1291)
<=>
{out_attr1.x = float(0.1291)
}
*/
	out_attr1.y  = float(0.14181 );	/* 0.14181  <=>  float(0.14181)
<=>
float(0.14181)
<=>
{out_attr1.y = float(0.14181)
}
*/
	out_attr1.z  = float(0.14524 );	/* 0.14524  <=>  float(0.14524)
<=>
float(0.14524)
<=>
{out_attr1.z = float(0.14524)
}
*/
	out_attr1.w  = float(1.00 );	/* 1.00  <=>  float(1.00)
<=>
float(1.00)
<=>
{out_attr1.w = float(1.00)
}
*/
	out_attr2.x  = float(0.60233 );	/* 0.60233  <=>  float(0.60233)
<=>
float(0.60233)
<=>
{out_attr2.x = float(0.60233)
}
*/
	out_attr2.y  = float(1.42053 );	/* 1.42053  <=>  float(1.42053)
<=>
float(1.42053)
<=>
{out_attr2.y = float(1.42053)
}
*/
	out_attr2.z  = float(0.50196 );	/* 0.50196  <=>  float(0.50196)
<=>
float(0.50196)
<=>
{out_attr2.z = float(0.50196)
}
*/
	out_attr2.w  = float(0.50196 );	/* 0.50196  <=>  float(0.50196)
<=>
float(0.50196)
<=>
{out_attr2.w = float(0.50196)
}
*/
	out_attr3.x  = float(-188.93994 );	/* -188.93994  <=>  float(-188.93994)
<=>
float(-188.93994)
<=>
{out_attr3.x = float(-188.93994)
}
*/
	out_attr3.y  = float(345.52649 );	/* 345.5265  <=>  float(345.52649)
<=>
float(345.52649)
<=>
{out_attr3.y = float(345.52649)
}
*/
	out_attr3.z  = float(489.76498 );	/* 489.765  <=>  float(489.76498)
<=>
float(489.76498)
<=>
{out_attr3.z = float(489.76498)
}
*/
	out_attr3.w  = float(489.86304 );	/* 489.863  <=>  float(489.86304)
<=>
float(489.86304)
<=>
{out_attr3.w = float(489.86304)
}
*/
	out_attr4.x  = float(1.00 );	/* 1.00  <=>  float(1.00)
<=>
float(1.00)
<=>
{out_attr4.x = float(1.00)
}
*/
	out_attr4.y  = float(0.00 );	/* 0  <=>  float(0.00)
<=>
float(0.00)
<=>
{out_attr4.y = float(0.00)
}
*/
	out_attr4.z  = float(0.00 );	/* 0  <=>  float(0.00)
<=>
float(0.00)
<=>
{out_attr4.z = float(0.00)
}
*/
	out_attr4.w  = float(1.00 );	/* 1.00  <=>  float(1.00)
<=>
float(1.00)
<=>
{out_attr4.w = float(1.00)
}
*/
	out_attr5.x  = float(1.00 );	/* 1.00  <=>  float(1.00)
<=>
float(1.00)
<=>
{out_attr5.x = float(1.00)
}
*/
	out_attr5.y  = float(1.00 );	/* 1.00  <=>  float(1.00)
<=>
float(1.00)
<=>
{out_attr5.y = float(1.00)
}
*/
	out_attr5.z  = float(1.00 );	/* 1.00  <=>  float(1.00)
<=>
float(1.00)
<=>
{out_attr5.z = float(1.00)
}
*/
	out_attr5.w  = float(1.00 );	/* 1.00  <=>  float(1.00)
<=>
float(1.00)
<=>
{out_attr5.w = float(1.00)
}
*/
	out_attr6.x  = float(0.00 );	/* 0  <=>  float(0.00)
<=>
float(0.00)
<=>
{out_attr6.x = float(0.00)
}
*/
	out_attr6.y  = float(1.00 );	/* 1.00  <=>  float(1.00)
<=>
float(1.00)
<=>
{out_attr6.y = float(1.00)
}
*/
	out_attr6.z  = float(0.00 );	/* 0  <=>  float(0.00)
<=>
float(0.00)
<=>
{out_attr6.z = float(0.00)
}
*/
	out_attr6.w  = float(1.00 );	/* 1.00  <=>  float(1.00)
<=>
float(1.00)
<=>
{out_attr6.w = float(1.00)
}
*/
	out_attr7.x  = float(1.00 );	/* 1.00  <=>  float(1.00)
<=>
float(1.00)
<=>
{out_attr7.x = float(1.00)
}
*/
	out_attr7.y  = float(0.00 );	/* 0  <=>  float(0.00)
<=>
float(0.00)
<=>
{out_attr7.y = float(0.00)
}
*/
	out_attr7.z  = float(0.00 );	/* 0  <=>  float(0.00)
<=>
float(0.00)
<=>
{out_attr7.z = float(0.00)
}
*/
	out_attr7.w  = float(1.00 );	/* 1.00  <=>  float(1.00)
<=>
float(1.00)
<=>
{out_attr7.w = float(1.00)
}
*/
	out_attr8.x  = float(0.70066 );	/* 0.70066  <=>  float(0.70066)
<=>
float(0.70066)
<=>
{out_attr8.x = float(0.70066)
}
*/
	out_attr8.y  = float(0.20 );	/* 0.20  <=>  float(0.20)
<=>
float(0.20)
<=>
{out_attr8.y = float(0.20)
}
*/
	out_attr8.z  = float(0.00 );	/* 0  <=>  float(0.00)
<=>
float(0.00)
<=>
{out_attr8.z = float(0.00)
}
*/
	out_attr8.w  = float(0.00 );	/* 0  <=>  float(0.00)
<=>
float(0.00)
<=>
{out_attr8.w = float(0.00)
}
*/
	out_attr9.x  = float(0.10241 );	/* 0.10241  <=>  float(0.10241)
<=>
float(0.10241)
<=>
{out_attr9.x = float(0.10241)
}
*/
	out_attr9.y  = float(0.10241 );	/* 0.10241  <=>  float(0.10241)
<=>
float(0.10241)
<=>
{out_attr9.y = float(0.10241)
}
*/
	out_attr9.z  = float(0.10241 );	/* 0.10241  <=>  float(0.10241)
<=>
float(0.10241)
<=>
{out_attr9.z = float(0.10241)
}
*/
	out_attr9.w  = float(1.00 );	/* 1.00  <=>  float(1.00)
<=>
float(1.00)
<=>
{out_attr9.w = float(1.00)
}
*/
	out_attr10.x  = float(0.00111 );	/* 0.00111  <=>  float(0.00111)
<=>
float(0.00111)
<=>
{out_attr10.x = float(0.00111)
}
*/
	out_attr10.y  = float(0.00877 );	/* 0.00877  <=>  float(0.00877)
<=>
float(0.00877)
<=>
{out_attr10.y = float(0.00877)
}
*/
	out_attr10.z  = float(0.00901 );	/* 0.00901  <=>  float(0.00901)
<=>
float(0.00901)
<=>
{out_attr10.z = float(0.00901)
}
*/
	out_attr10.w  = float(0.2585 );	/* 0.2585  <=>  float(0.2585)
<=>
float(0.2585)
<=>
{out_attr10.w = float(0.2585)
}
*/
	vs_cbuf0[21 ] = vec4(1.0935697E-14, 8E-45, 8.2508E-41, 0 );	// vec4(1.0935697E-14,8E-45,8.2508E-41,0)
	vs_cbuf8[0 ] = vec4(-0.7425708, 1.493044E-08, 0.6697676, 1075.086 );	// vec4(-0.7425708,1.493044E-08,0.6697676,1075.086)
	vs_cbuf8[1 ] = vec4(0.339885, 0.8616711, 0.3768303, 1743.908 );	// vec4(0.339885,0.8616711,0.3768303,1743.908)
	vs_cbuf8[2 ] = vec4(-0.57711935, 0.5074672, -0.6398518, -3681.8398 );	// vec4(-0.57711935,0.5074672,-0.6398518,-3681.8398)
	vs_cbuf8[3 ] = vec4(0, 0, 0, 1.00 );	// vec4(0,0,0,1.00)
	vs_cbuf8[4 ] = vec4(1.206285, 0, 0, 0 );	// vec4(1.206285,0,0,0)
	vs_cbuf8[5 ] = vec4(0, 2.144507, 0, 0 );	// vec4(0,2.144507,0,0)
	vs_cbuf8[6 ] = vec4(0, 0, -1.000008, -0.2000008 );	// vec4(0,0,-1.000008,-0.2000008)
	vs_cbuf8[7 ] = vec4(0, 0, -1, 0 );	// vec4(0,0,-1,0)
	vs_cbuf8[29 ] = vec4(-1919.2622, 365.7373, -3733.0469, 0 );	// vec4(-1919.2622,365.7373,-3733.0469,0)
	vs_cbuf8[30 ] = vec4(0.10, 25000.00, 2500.00, 24999.90 );	// vec4(0.10,25000.00,2500.00,24999.90)
	vs_cbuf9[0 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[1 ] = uvec4(1851878512, 101, 0, 0 );	// uvec4(1851878512,101,0,0)
	vs_cbuf9[2 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[3 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[4 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[5 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[6 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[7 ] = uvec4(1, 0, 299520, 0 );	// uvec4(1,0,299520,0)
	vs_cbuf9[8 ] = uvec4(0, 1, 0, 1 );	// uvec4(0,1,0,1)
	vs_cbuf9[9 ] = uvec4(1, 0, 0, 0 );	// uvec4(1,0,0,0)
	vs_cbuf9[10 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[11 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[12 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[13 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[14 ] = uvec4(0, 3212836864, 0, 0 );	// uvec4(0,3212836864,0,0)
	vs_cbuf9[15 ] = uvec4(1065353216, 0, 0, 0 );	// uvec4(1065353216,0,0,0)
	vs_cbuf9[16 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[17 ] = uvec4(1065353216, 1065353216, 1101004800, 1101004800 );	// uvec4(1065353216,1065353216,1101004800,1101004800)
	vs_cbuf9[18 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[19 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[20 ] = uvec4(1073741824, 1065353216, 1065353216, 1065353216 );	// uvec4(1073741824,1065353216,1065353216,1065353216)
	vs_cbuf9[21 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[22 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[23 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[24 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[25 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[26 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[27 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[28 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[29 ] = uvec4(1073741824, 1065353216, 1065353216, 1065353216 );	// uvec4(1073741824,1065353216,1065353216,1065353216)
	vs_cbuf9[30 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[31 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[32 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[33 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[34 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[35 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[36 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[37 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[38 ] = uvec4(1073741824, 1065353216, 1065353216, 1065353216 );	// uvec4(1073741824,1065353216,1065353216,1065353216)
	vs_cbuf9[39 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[40 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[41 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[42 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[43 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[44 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[45 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[46 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[47 ] = uvec4(1082130432, 1065353216, 1065353216, 1065353216 );	// uvec4(1082130432,1065353216,1065353216,1065353216)
	vs_cbuf9[48 ] = uvec4(0, 1, 2, 3 );	// uvec4(0,1,2,3)
	vs_cbuf9[49 ] = uvec4(4, 5, 6, 7 );	// uvec4(4,5,6,7)
	vs_cbuf9[50 ] = uvec4(8, 9, 10, 11 );	// uvec4(8,9,10,11)
	vs_cbuf9[51 ] = uvec4(12, 13, 14, 15 );	// uvec4(12,13,14,15)
	vs_cbuf9[52 ] = uvec4(16, 17, 18, 19 );	// uvec4(16,17,18,19)
	vs_cbuf9[53 ] = uvec4(20, 21, 22, 23 );	// uvec4(20,21,22,23)
	vs_cbuf9[54 ] = uvec4(24, 25, 26, 27 );	// uvec4(24,25,26,27)
	vs_cbuf9[55 ] = uvec4(28, 29, 30, 31 );	// uvec4(28,29,30,31)
	vs_cbuf9[56 ] = uvec4(1082130432, 1065353216, 1065353216, 1065353216 );	// uvec4(1082130432,1065353216,1065353216,1065353216)
	vs_cbuf9[57 ] = uvec4(0, 1, 2, 3 );	// uvec4(0,1,2,3)
	vs_cbuf9[58 ] = uvec4(4, 5, 6, 7 );	// uvec4(4,5,6,7)
	vs_cbuf9[59 ] = uvec4(8, 9, 10, 11 );	// uvec4(8,9,10,11)
	vs_cbuf9[60 ] = uvec4(12, 13, 14, 15 );	// uvec4(12,13,14,15)
	vs_cbuf9[61 ] = uvec4(16, 17, 18, 19 );	// uvec4(16,17,18,19)
	vs_cbuf9[62 ] = uvec4(20, 21, 22, 23 );	// uvec4(20,21,22,23)
	vs_cbuf9[63 ] = uvec4(24, 25, 26, 27 );	// uvec4(24,25,26,27)
	vs_cbuf9[64 ] = uvec4(28, 29, 30, 31 );	// uvec4(28,29,30,31)
	vs_cbuf9[65 ] = uvec4(1082130432, 1065353216, 1065353216, 1065353216 );	// uvec4(1082130432,1065353216,1065353216,1065353216)
	vs_cbuf9[66 ] = uvec4(0, 1, 2, 3 );	// uvec4(0,1,2,3)
	vs_cbuf9[67 ] = uvec4(4, 5, 6, 7 );	// uvec4(4,5,6,7)
	vs_cbuf9[68 ] = uvec4(8, 9, 10, 11 );	// uvec4(8,9,10,11)
	vs_cbuf9[69 ] = uvec4(12, 13, 14, 15 );	// uvec4(12,13,14,15)
	vs_cbuf9[70 ] = uvec4(16, 17, 18, 19 );	// uvec4(16,17,18,19)
	vs_cbuf9[71 ] = uvec4(20, 21, 22, 23 );	// uvec4(20,21,22,23)
	vs_cbuf9[72 ] = uvec4(24, 25, 26, 27 );	// uvec4(24,25,26,27)
	vs_cbuf9[73 ] = uvec4(28, 29, 30, 31 );	// uvec4(28,29,30,31)
	vs_cbuf9[74 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[75 ] = uvec4(1065353216, 1065353216, 0, 0 );	// uvec4(1065353216,1065353216,0,0)
	vs_cbuf9[76 ] = uvec4(1073741824, 1073741824, 0, 0 );	// uvec4(1073741824,1073741824,0,0)
	vs_cbuf9[77 ] = uvec4(979682184, 0, 0, 0 );	// uvec4(979682184,0,0,0)
	vs_cbuf9[78 ] = uvec4(1065353216, 1065353216, 1065353216, 1065353216 );	// uvec4(1065353216,1065353216,1065353216,1065353216)
	vs_cbuf9[79 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[80 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[81 ] = uvec4(1065353216, 1065353216, 0, 0 );	// uvec4(1065353216,1065353216,0,0)
	vs_cbuf9[82 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[83 ] = uvec4(1065353216, 1065353216, 1065353216, 1065353216 );	// uvec4(1065353216,1065353216,1065353216,1065353216)
	vs_cbuf9[84 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[85 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[86 ] = uvec4(1065353216, 1065353216, 0, 0 );	// uvec4(1065353216,1065353216,0,0)
	vs_cbuf9[87 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[88 ] = uvec4(1065353216, 1065353216, 1065353216, 1065353216 );	// uvec4(1065353216,1065353216,1065353216,1065353216)
	vs_cbuf9[89 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[90 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[91 ] = uvec4(1065353216, 1065353216, 0, 0 );	// uvec4(1065353216,1065353216,0,0)
	vs_cbuf9[92 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[93 ] = uvec4(1065353216, 1065353216, 1065353216, 1065353216 );	// uvec4(1065353216,1065353216,1065353216,1065353216)
	vs_cbuf9[94 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[95 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[96 ] = uvec4(1065353216, 1065353216, 0, 0 );	// uvec4(1065353216,1065353216,0,0)
	vs_cbuf9[97 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[98 ] = uvec4(1065353216, 1065353216, 1065353216, 1065353216 );	// uvec4(1065353216,1065353216,1065353216,1065353216)
	vs_cbuf9[99 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[100 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[101 ] = uvec4(1065353216, 1065353216, 0, 0 );	// uvec4(1065353216,1065353216,0,0)
	vs_cbuf9[102 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[103 ] = uvec4(1065353216, 1065353216, 1065353216, 1065353216 );	// uvec4(1065353216,1065353216,1065353216,1065353216)
	vs_cbuf9[104 ] = uvec4(1050253722, 0, 0, 0 );	// uvec4(1050253722,0,0,0)
	vs_cbuf9[105 ] = uvec4(1062794785, 1063888538, 1063862710, 0 );	// uvec4(1062794785,1063888538,1063862710,0)
	vs_cbuf9[106 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[107 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[108 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[109 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[110 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[111 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[112 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[113 ] = uvec4(1069547520, 1065353216, 1065353216, 0 );	// uvec4(1069547520,1065353216,1065353216,0)
	vs_cbuf9[114 ] = uvec4(1069547520, 1065353216, 1065353216, 1065353216 );	// uvec4(1069547520,1065353216,1065353216,1065353216)
	vs_cbuf9[115 ] = uvec4(1069547520, 1065353216, 1065353216, 1073741824 );	// uvec4(1069547520,1065353216,1065353216,1073741824)
	vs_cbuf9[116 ] = uvec4(1069547520, 1065353216, 1065353216, 1077936128 );	// uvec4(1069547520,1065353216,1065353216,1077936128)
	vs_cbuf9[117 ] = uvec4(1069547520, 1065353216, 1065353216, 1082130432 );	// uvec4(1069547520,1065353216,1065353216,1082130432)
	vs_cbuf9[118 ] = uvec4(1069547520, 1065353216, 1065353216, 1084227584 );	// uvec4(1069547520,1065353216,1065353216,1084227584)
	vs_cbuf9[119 ] = uvec4(1069547520, 1065353216, 1065353216, 1086324736 );	// uvec4(1069547520,1065353216,1065353216,1086324736)
	vs_cbuf9[120 ] = uvec4(1069547520, 1065353216, 1065353216, 1088421888 );	// uvec4(1069547520,1065353216,1065353216,1088421888)
	vs_cbuf9[121 ] = uvec4(1054627042, 1056048280, 1056431999, 0 );	// uvec4(1054627042,1056048280,1056431999,0)
	vs_cbuf9[122 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[123 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[124 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[125 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[126 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[127 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[128 ] = uvec4(0, 0, 0, 0 );	// uvec4(0,0,0,0)
	vs_cbuf9[129 ] = uvec4(1065353216, 1065353216, 1065353216, 0 );	// uvec4(1065353216,1065353216,1065353216,0)
	vs_cbuf9[130 ] = uvec4(1065353216, 1065353216, 1065353216, 1065353216 );	// uvec4(1065353216,1065353216,1065353216,1065353216)
	vs_cbuf9[131 ] = uvec4(1065353216, 1065353216, 1065353216, 1073741824 );	// uvec4(1065353216,1065353216,1065353216,1073741824)
	vs_cbuf9[132 ] = uvec4(1065353216, 1065353216, 1065353216, 1077936128 );	// uvec4(1065353216,1065353216,1065353216,1077936128)
	vs_cbuf9[133 ] = uvec4(1065353216, 1065353216, 1065353216, 1082130432 );	// uvec4(1065353216,1065353216,1065353216,1082130432)
	vs_cbuf9[134 ] = uvec4(1065353216, 1065353216, 1065353216, 1084227584 );	// uvec4(1065353216,1065353216,1065353216,1084227584)
	vs_cbuf9[135 ] = uvec4(1065353216, 1065353216, 1065353216, 1086324736 );	// uvec4(1065353216,1065353216,1065353216,1086324736)
	vs_cbuf9[136 ] = uvec4(1065353216, 1065353216, 1065353216, 1088421888 );	// uvec4(1065353216,1065353216,1065353216,1088421888)
	vs_cbuf9[137 ] = uvec4(0, 1056964608, 1065353216, 0 );	// uvec4(0,1056964608,1065353216,0)
	vs_cbuf9[138 ] = uvec4(1092616192, 1106247680, 1117782016, 1120403456 );	// uvec4(1092616192,1106247680,1117782016,1120403456)
	vs_cbuf9[139 ] = uvec4(1065353216, 0, 0, 0 );	// uvec4(1065353216,0,0,0)
	vs_cbuf9[140 ] = uvec4(0, 1120403456, 0, 0 );	// uvec4(0,1120403456,0,0)
	vs_cbuf9[141 ] = uvec4(1065353216, 1065353216, 1065353216, 0 );	// uvec4(1065353216,1065353216,1065353216,0)
	vs_cbuf10[0 ] = vec4(1.00, 1.00, 1.00, 1.00 );	// vec4(1.00,1.00,1.00,1.00)
	vs_cbuf10[1 ] = vec4(1.00, 1.00, 1.00, 1.00 );	// vec4(1.00,1.00,1.00,1.00)
	vs_cbuf10[2 ] = vec4(999.50, 1.00, 1.00, 1.00 );	// vec4(999.50,1.00,1.00,1.00)
	vs_cbuf10[3 ] = vec4(1.00, 1.00, 1.00, 1.00 );	// vec4(1.00,1.00,1.00,1.00)
	vs_cbuf10[4 ] = vec4(1.00, 0, 0, -1134.2701 );	// vec4(1.00,0,0,-1134.2701)
	vs_cbuf10[5 ] = vec4(0, 1.00, 0, 35.58958 );	// vec4(0,1.00,0,35.58958)
	vs_cbuf10[6 ] = vec4(0, 0, 1.00, -3936.7583 );	// vec4(0,0,1.00,-3936.7583)
	vs_cbuf10[7 ] = vec4(0, 0, 0, 1.00 );	// vec4(0,0,0,1.00)
	vs_cbuf10[8 ] = vec4(1.00, 0, 0, -1134.2701 );	// vec4(1.00,0,0,-1134.2701)
	vs_cbuf10[9 ] = vec4(0, 1.00, 0, 35.58958 );	// vec4(0,1.00,0,35.58958)
	vs_cbuf10[10 ] = vec4(0, 0, 1.00, -3936.7583 );	// vec4(0,0,1.00,-3936.7583)
	vs_cbuf13[0 ] = vec4(0, 1.00, 1.00, 1.00 );	// vec4(0,1.00,1.00,1.00)
	vs_cbuf15[22 ] = vec4(0.0000333, -0.0016638935, 0, 0 );	// vec4(0.0000333,-0.0016638935,0,0)
	vs_cbuf15[23 ] = vec4(20.00, 1.00, 0.85, -0.010725529 );	// vec4(20.00,1.00,0.85,-0.010725529)
	vs_cbuf15[24 ] = vec4(0.002381, -0.04761905, 3.363175, 4.00 );	// vec4(0.002381,-0.04761905,3.363175,4.00)
	vs_cbuf15[25 ] = vec4(0.0282744, 0.0931012, 0.1164359, 0.7006614 );	// vec4(0.0282744,0.0931012,0.1164359,0.7006614)
	vs_cbuf15[26 ] = vec4(0.0174636, 0.1221582, 0.2193998, 0.20 );	// vec4(0.0174636,0.1221582,0.2193998,0.20)
	vs_cbuf15[27 ] = vec4(-0.14285715, 0.0071429, 250.00, 0 );	// vec4(-0.14285715,0.0071429,250.00,0)
	vs_cbuf15[28 ] = vec4(0.8802994, -0.4663191, -0.08728968, 0 );	// vec4(0.8802994,-0.4663191,-0.08728968,0)
	vs_cbuf15[49 ] = vec4(0, 0, 0, 0 );	// vec4(0,0,0,0)
	vs_cbuf15[51 ] = vec4(950.00, 50.00, 1.50, 1.00 );	// vec4(950.00,50.00,1.50,1.00)
	vs_cbuf15[52 ] = vec4(-2116, -3932, 0.0025, 0 );	// vec4(-2116,-3932,0.0025,0)
	vs_cbuf15[58 ] = vec4(1.00, 1.00, 1.00, 1.00 );	// vec4(1.00,1.00,1.00,1.00)
	bool b_0 = bool(0 );	// False
	bool b_1 = bool(0 );	// False
	bool b_2 = bool(0 );	// False
	bool b_3 = bool(0 );	// False
	bool b_4 = bool(0 );	// False
	bool b_5 = bool(0 );	// False
	bool b_6 = bool(0 );	// False
	bool b_7 = bool(0 );	// False
	uint u_0 = uint(0 );	// 0
	uint u_1 = uint(0 );	// 0
	uint u_2 = uint(0 );	// 0
	uint u_3 = uint(0 );	// 0
	uint u_4 = uint(0 );	// 0
	uint u_5 = uint(0 );	// 0
	uint u_6 = uint(0 );	// 0
	uint u_7 = uint(0 );	// 0
	uint u_8 = uint(0 );	// 0
	uint u_9 = uint(0 );	// 0
	uint u_10 = uint(0 );	// 0
	uint u_11 = uint(0 );	// 0
	uint u_12 = uint(0 );	// 0
	uint u_13 = uint(0 );	// 0
	uint u_14 = uint(0 );	// 0
	uint u_15 = uint(0 );	// 0
	uint u_16 = uint(0 );	// 0
	uint u_17 = uint(0 );	// 0
	uint u_18 = uint(0 );	// 0
	uint u_19 = uint(0 );	// 0
	uint u_20 = uint(0 );	// 0
	uint u_21 = uint(0 );	// 0
	uint u_22 = uint(0 );	// 0
	uint u_23 = uint(0 );	// 0
	uint u_24 = uint(0 );	// 0
	uint u_25 = uint(0 );	// 0
	float f_0 = float(0 );	// 0
	float f_1 = float(0 );	// 0
	float f_2 = float(0 );	// 0
	float f_3 = float(0 );	// 0
	float f_4 = float(0 );	// 0
	float f_5 = float(0 );	// 0
	float f_6 = float(0 );	// 0
	float f_7 = float(0 );	// 0
	float f_8 = float(0 );	// 0
	float f_9 = float(0 );	// 0
	float f_10 = float(0 );	// 0
	float f_11 = float(0 );	// 0
	float f_12 = float(0 );	// 0
	float f_13 = float(0 );	// 0
	float f_14 = float(0 );	// 0
	float f_15 = float(0 );	// 0
	float f_16 = float(0 );	// 0
	float f_17 = float(0 );	// 0
	float f_18 = float(0 );	// 0
	float f_19 = float(0 );	// 0
	float f_20 = float(0 );	// 0
	vec2 f2_0 = vec2(0 );	// vec2(0,0)
	uvec4 u4_0 = uvec4(0 );	// uvec4(0,0,0,0)
	vec4 f4_0 = vec4(0 );	// vec4(0,0,0,0)
	precise float pf_0 = float(0 );	// 0
	precise float pf_1 = float(0 );	// 0
	precise float pf_2 = float(0 );	// 0
	precise float pf_3 = float(0 );	// 0
	precise float pf_4 = float(0 );	// 0
	precise float pf_5 = float(0 );	// 0
	precise float pf_6 = float(0 );	// 0
	precise float pf_7 = float(0 );	// 0
	precise float pf_8 = float(0 );	// 0
	precise float pf_9 = float(0 );	// 0
	precise float pf_10 = float(0 );	// 0
	precise float pf_11 = float(0 );	// 0
	precise float pf_12 = float(0 );	// 0
	precise float pf_13 = float(0 );	// 0
	precise float pf_14 = float(0 );	// 0
	precise float pf_15 = float(0 );	// 0
	precise float pf_16 = float(0 );	// 0
	precise float pf_17 = float(0 );	// 0
	precise float pf_18 = float(0 );	// 0
	precise float pf_19 = float(0 );	// 0
	precise float pf_20 = float(0 );	// 0
	u_0_0 = 0u;	// 0
	u_1_0 = 0u;	// 0
	u_2_0 = 0u;	// 0
	gl_Position = vec4(0, 0, 0, 1 );	// vec4(0,0,0,1.00)
	out_attr0 = vec4(0, 0, 0, 1 );	// vec4(0,0,0,1.00)
	out_attr1 = vec4(0, 0, 0, 1 );	// vec4(0,0,0,1.00)
	out_attr2 = vec4(0, 0, 0, 1 );	// vec4(0,0,0,1.00)
	out_attr3 = vec4(0, 0, 0, 1 );	// vec4(0,0,0,1.00)
	out_attr4 = vec4(0, 0, 0, 1 );	// vec4(0,0,0,1.00)
	out_attr5 = vec4(0, 0, 0, 1 );	// vec4(0,0,0,1.00)
	out_attr6 = vec4(0, 0, 0, 1 );	// vec4(0,0,0,1.00)
	out_attr7 = vec4(0, 0, 0, 1 );	// vec4(0,0,0,1.00)
	out_attr8 = vec4(0, 0, 0, 1 );	// vec4(0,0,0,1.00)
	out_attr9 = vec4(0, 0, 0, 1 );	// vec4(0,0,0,1.00)
	out_attr10 = vec4(0, 0, 0, 1 );	// vec4(0,0,0,1.00)
	f_0_0 = in_attr4.w ;	// 268435000.00
	f_1_0 = trunc(f_0_0 );	// 268435000.00
	f_1_1 = min(max(f_1_0, float(-2147483600.f ) ), float(2147483600.f ) );	// 268435000.00
	u_3_0 = int(f_1_1 );	// 268435008
	b_0_0 = isnan(f_0_0 );	// False
	u_3_1 = b_0_0 ? (0u) : (u_3_0);	// 268435008
	b_0_1 = int(u_3_1 ) <= int(0u );	// False
	b_1_0 = b_0_1 ? (true) : (false);	// False
	if(b_1_0 )	/* False  <=>  if(((int((isnan({in_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({in_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
<=>
if(((int((isnan(in_attr4.w) ? 0u : int(clamp(trunc(in_attr4.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
<=>if(b_1_0...)
*/
	{
		gl_Position.x  = 0.f;	/* 0  <=>  0.f
<=>
0.f
<=>
{gl_Position.x = 0.f
}
*/
	} 
	b_1_1 = b_0_1 ? (true) : (false);	// False
	u_4_0 = u_2_0;	/* 0  <=>  0u
<=>
0u
<=>
{u_4_0 = 
	{u_2_0 = 0u
	}
}
*/
	u_5_phi_2 = u_5;
	u_4_phi_2 = u_4_0;
	if(b_1_1 )	/* False  <=>  if(((int((isnan({in_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({in_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
<=>
if(((int((isnan(in_attr4.w) ? 0u : int(clamp(trunc(in_attr4.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
<=>if(b_1_1...)
*/
	{
		u_5_0 = (vs_cbuf8[30 ].y  );	// 1187205120
		u_4_1 = u_5_0;	/* 1187205120  <=>  ftou({vs_cbuf8_30.y : 25000.00})
<=>
ftou(vs_cbuf8_30.y)
<=>
{u_4_1 = 
	{u_5_0 = ftou(vs_cbuf8_30.y)
	}
}
*/
		u_5_phi_2 = u_5_0;
		u_4_phi_2 = u_4_1;
	} 
	b_1_2 = b_0_1 ? (true) : (false);	// False
	if(b_1_2 )	/* False  <=>  if(((int((isnan({in_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({in_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
<=>
if(((int((isnan(in_attr4.w) ? 0u : int(clamp(trunc(in_attr4.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
<=>if(b_1_2...)
*/
	{
		gl_Position.y  = 0.f;	/* 0  <=>  0.f
<=>
0.f
<=>
{gl_Position.y = 0.f
}
*/
	} 
	b_1_3 = b_0_1 ? (true) : (false);	// False
	u_2_1 = u_4_phi_2;	/* 0  <=>  {u_4_phi_2 : 0}
<=>
u_4_phi_2
<=>
{u_2_1 = u_4_phi_2
}
*/
	f_0_phi_4 = f_0_0;
	pf_0_phi_4 = pf_0;
	u_5_phi_4 = u_5;
	u_2_phi_4 = u_2_1;
	if(b_1_3 )	/* False  <=>  if(((int((isnan({in_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({in_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
<=>
if(((int((isnan(in_attr4.w) ? 0u : int(clamp(trunc(in_attr4.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
<=>if(b_1_3...)
*/
	{
		f_0_1 = utof(u_4_phi_2 );	// 0
		pf_0_0 = f_0_1 * 5.f;	// 0
		u_5_1 = ftou(pf_0_0 );	// 0
		u_2_2 = u_5_1;	/* 0  <=>  {ftou(({utof(u_4_phi_2) : 0} * 5.f)) : 0}
<=>
ftou((utof(u_4_phi_2) * 5.f))
<=>
{u_2_2 = 
	{u_5_1 = ftou(
		{pf_0_0 = (
			{f_0_1 = utof(u_4_phi_2)
			} * 5.f)
		})
	}
}
*/
		f_0_phi_4 = f_0_1;
		pf_0_phi_4 = pf_0_0;
		u_5_phi_4 = u_5_1;
		u_2_phi_4 = u_2_2;
	} 
	b_1_4 = b_0_1 ? (true) : (false);	// False
	if(b_1_4 )	/* False  <=>  if(((int((isnan({in_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({in_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
<=>
if(((int((isnan(in_attr4.w) ? 0u : int(clamp(trunc(in_attr4.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
<=>if(b_1_4...)
*/
	{
		out_attr4.x  = 0.f;	/* 0  <=>  0.f
<=>
0.f
<=>
{out_attr4.x = 0.f
}
*/
	} 
	b_1_5 = b_0_1 ? (true) : (false);	// False
	f_0_phi_6 = f_0_phi_4;
	if(b_1_5 )	/* False  <=>  if(((int((isnan({in_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({in_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
<=>
if(((int((isnan(in_attr4.w) ? 0u : int(clamp(trunc(in_attr4.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
<=>if(b_1_5...)
*/
	{
		f_0_2 = utof(u_2_phi_4 );	// 0
		gl_Position.z  = f_0_2;	/* 0  <=>  {utof(u_2_phi_4) : 0}
<=>
utof(u_2_phi_4)
<=>
{gl_Position.z = 
	{f_0_2 = utof(u_2_phi_4)
	}
}
*/
		f_0_phi_6 = f_0_2;
	} 
	b_1_6 = b_0_1 ? (true) : (false);	// False
	if(b_1_6 )	/* False  <=>  if(((int((isnan({in_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({in_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
<=>
if(((int((isnan(in_attr4.w) ? 0u : int(clamp(trunc(in_attr4.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
<=>if(b_1_6...)
*/
	{
		return;
	} 
	f_0_3 = in_attr5.w ;	// 0
	u_2_3 = ftou(f_0_3 );	// 0
	f_1_2 = float(int(u_3_1 ) );	// 268435000.00
	u_3_2 = ftou(f_1_2 );	// 1300234212
	f_2_0 = utof(vs_cbuf10[2 ].x  );	// 999.50
	f_3_0 = 0.f - (f_0_3 );	// 0
	pf_0_1 = f_3_0 + f_2_0;	/* 999.50  <=>  ((0.f - {in_attr5.w : 0}) + {(vs_cbuf10_2.x) : 999.50})
<=>
((0.f - in_attr5.w) + (vs_cbuf10_2.x))
<=>
{pf_0_1 = (
	{f_3_0 = (0.f - 
		{f_0_3 = in_attr5.w
		})
	} + 
	{f_2_0 = (vs_cbuf10_2.x)
	})
}
*/
	b_0_2 = pf_0_1 >= f_1_2 && ! isnan(pf_0_1 ) && ! isnan(f_1_2 );	// False
	f_1_3 = utof(vs_cbuf10[2 ].x  );	// 999.50
	b_1_7 = f_0_3 > f_1_3 && ! isnan(f_0_3 ) && ! isnan(f_1_3 );	// False
	b_0_3 = b_1_7 || b_0_2;	// False
	b_1_8 = b_0_3 ? (true) : (false);	// False
	if(b_1_8 )	/* False  <=>  if(((((({in_attr5.w : 0} > {(vs_cbuf10_2.x) : 999.50}) && (! isnan({in_attr5.w : 0}))) && (! isnan({(vs_cbuf10_2.x) : 999.50}))) || ((({pf_0_1 : 999.50} >= float(int((isnan({in_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({in_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f))))))) && (! isnan({pf_0_1 : 999.50}))) && (! isnan(float(int((isnan({in_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({in_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
<=>
if((((((in_attr5.w > (vs_cbuf10_2.x)) && (! isnan(in_attr5.w))) && (! isnan((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((isnan(in_attr4.w) ? 0u : int(clamp(trunc(in_attr4.w), float(-2147483600.f), float(2147483600.f))))))) && (! isnan(pf_0_1))) && (! isnan(float(int((isnan(in_attr4.w) ? 0u : int(clamp(trunc(in_attr4.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
<=>if(b_1_8...)
*/
	{
		gl_Position.x  = 0.f;	/* 0  <=>  0.f
<=>
0.f
<=>
{gl_Position.x = 0.f
}
*/
	} 
	b_1_9 = b_0_3 ? (true) : (false);	// False
	u_4_2 = u_2_3;	/* 0  <=>  {ftou(in_attr5.w) : 0}
<=>
ftou(in_attr5.w)
<=>
{u_4_2 = 
	{u_2_3 = ftou(
		{f_0_3 = in_attr5.w
		})
	}
}
*/
	u_5_phi_9 = u_5;
	u_4_phi_9 = u_4_2;
	if(b_1_9 )	/* False  <=>  if(((((({in_attr5.w : 0} > {(vs_cbuf10_2.x) : 999.50}) && (! isnan({in_attr5.w : 0}))) && (! isnan({(vs_cbuf10_2.x) : 999.50}))) || ((({pf_0_1 : 999.50} >= float(int((isnan({in_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({in_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f))))))) && (! isnan({pf_0_1 : 999.50}))) && (! isnan(float(int((isnan({in_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({in_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
<=>
if((((((in_attr5.w > (vs_cbuf10_2.x)) && (! isnan(in_attr5.w))) && (! isnan((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((isnan(in_attr4.w) ? 0u : int(clamp(trunc(in_attr4.w), float(-2147483600.f), float(2147483600.f))))))) && (! isnan(pf_0_1))) && (! isnan(float(int((isnan(in_attr4.w) ? 0u : int(clamp(trunc(in_attr4.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
<=>if(b_1_9...)
*/
	{
		u_5_2 = (vs_cbuf8[30 ].y  );	// 1187205120
		u_4_3 = u_5_2;	/* 1187205120  <=>  ftou({vs_cbuf8_30.y : 25000.00})
<=>
ftou(vs_cbuf8_30.y)
<=>
{u_4_3 = 
	{u_5_2 = ftou(vs_cbuf8_30.y)
	}
}
*/
		u_5_phi_9 = u_5_2;
		u_4_phi_9 = u_4_3;
	} 
	b_1_10 = b_0_3 ? (true) : (false);	// False
	if(b_1_10 )	/* False  <=>  if(((((({in_attr5.w : 0} > {(vs_cbuf10_2.x) : 999.50}) && (! isnan({in_attr5.w : 0}))) && (! isnan({(vs_cbuf10_2.x) : 999.50}))) || ((({pf_0_1 : 999.50} >= float(int((isnan({in_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({in_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f))))))) && (! isnan({pf_0_1 : 999.50}))) && (! isnan(float(int((isnan({in_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({in_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
<=>
if((((((in_attr5.w > (vs_cbuf10_2.x)) && (! isnan(in_attr5.w))) && (! isnan((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((isnan(in_attr4.w) ? 0u : int(clamp(trunc(in_attr4.w), float(-2147483600.f), float(2147483600.f))))))) && (! isnan(pf_0_1))) && (! isnan(float(int((isnan(in_attr4.w) ? 0u : int(clamp(trunc(in_attr4.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
<=>if(b_1_10...)
*/
	{
		gl_Position.y  = 0.f;	/* 0  <=>  0.f
<=>
0.f
<=>
{gl_Position.y = 0.f
}
*/
	} 
	b_1_11 = b_0_3 ? (true) : (false);	// False
	u_2_4 = u_4_phi_9;	/* 0  <=>  {u_4_phi_9 : 0}
<=>
u_4_phi_9
<=>
{u_2_4 = u_4_phi_9
}
*/
	f_0_phi_11 = f_0_3;
	pf_1_phi_11 = pf_1;
	u_5_phi_11 = u_5;
	u_2_phi_11 = u_2_4;
	if(b_1_11 )	/* False  <=>  if(((((({in_attr5.w : 0} > {(vs_cbuf10_2.x) : 999.50}) && (! isnan({in_attr5.w : 0}))) && (! isnan({(vs_cbuf10_2.x) : 999.50}))) || ((({pf_0_1 : 999.50} >= float(int((isnan({in_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({in_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f))))))) && (! isnan({pf_0_1 : 999.50}))) && (! isnan(float(int((isnan({in_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({in_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
<=>
if((((((in_attr5.w > (vs_cbuf10_2.x)) && (! isnan(in_attr5.w))) && (! isnan((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((isnan(in_attr4.w) ? 0u : int(clamp(trunc(in_attr4.w), float(-2147483600.f), float(2147483600.f))))))) && (! isnan(pf_0_1))) && (! isnan(float(int((isnan(in_attr4.w) ? 0u : int(clamp(trunc(in_attr4.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
<=>if(b_1_11...)
*/
	{
		f_0_4 = utof(u_4_phi_9 );	// 0
		pf_1_0 = f_0_4 * 5.f;	// 0
		u_5_3 = ftou(pf_1_0 );	// 0
		u_2_5 = u_5_3;	/* 0  <=>  {ftou(({utof(u_4_phi_9) : 0} * 5.f)) : 0}
<=>
ftou((utof(u_4_phi_9) * 5.f))
<=>
{u_2_5 = 
	{u_5_3 = ftou(
		{pf_1_0 = (
			{f_0_4 = utof(u_4_phi_9)
			} * 5.f)
		})
	}
}
*/
		f_0_phi_11 = f_0_4;
		pf_1_phi_11 = pf_1_0;
		u_5_phi_11 = u_5_3;
		u_2_phi_11 = u_2_5;
	} 
	b_1_12 = b_0_3 ? (true) : (false);	// False
	if(b_1_12 )	/* False  <=>  if(((((({in_attr5.w : 0} > {(vs_cbuf10_2.x) : 999.50}) && (! isnan({in_attr5.w : 0}))) && (! isnan({(vs_cbuf10_2.x) : 999.50}))) || ((({pf_0_1 : 999.50} >= float(int((isnan({in_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({in_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f))))))) && (! isnan({pf_0_1 : 999.50}))) && (! isnan(float(int((isnan({in_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({in_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
<=>
if((((((in_attr5.w > (vs_cbuf10_2.x)) && (! isnan(in_attr5.w))) && (! isnan((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((isnan(in_attr4.w) ? 0u : int(clamp(trunc(in_attr4.w), float(-2147483600.f), float(2147483600.f))))))) && (! isnan(pf_0_1))) && (! isnan(float(int((isnan(in_attr4.w) ? 0u : int(clamp(trunc(in_attr4.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
<=>if(b_1_12...)
*/
	{
		out_attr4.x  = 0.f;	/* 0  <=>  0.f
<=>
0.f
<=>
{out_attr4.x = 0.f
}
*/
	} 
	b_1_13 = b_0_3 ? (true) : (false);	// False
	f_0_phi_13 = f_0_phi_11;
	if(b_1_13 )	/* False  <=>  if(((((({in_attr5.w : 0} > {(vs_cbuf10_2.x) : 999.50}) && (! isnan({in_attr5.w : 0}))) && (! isnan({(vs_cbuf10_2.x) : 999.50}))) || ((({pf_0_1 : 999.50} >= float(int((isnan({in_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({in_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f))))))) && (! isnan({pf_0_1 : 999.50}))) && (! isnan(float(int((isnan({in_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({in_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
<=>
if((((((in_attr5.w > (vs_cbuf10_2.x)) && (! isnan(in_attr5.w))) && (! isnan((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((isnan(in_attr4.w) ? 0u : int(clamp(trunc(in_attr4.w), float(-2147483600.f), float(2147483600.f))))))) && (! isnan(pf_0_1))) && (! isnan(float(int((isnan(in_attr4.w) ? 0u : int(clamp(trunc(in_attr4.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
<=>if(b_1_13...)
*/
	{
		f_0_5 = utof(u_2_phi_11 );	// 0
		gl_Position.z  = f_0_5;	/* 0  <=>  {utof(u_2_phi_11) : 0}
<=>
utof(u_2_phi_11)
<=>
{gl_Position.z = 
	{f_0_5 = utof(u_2_phi_11)
	}
}
*/
		f_0_phi_13 = f_0_5;
	} 
	b_1_14 = b_0_3 ? (true) : (false);	// False
	if(b_1_14 )	/* False  <=>  if(((((({in_attr5.w : 0} > {(vs_cbuf10_2.x) : 999.50}) && (! isnan({in_attr5.w : 0}))) && (! isnan({(vs_cbuf10_2.x) : 999.50}))) || ((({pf_0_1 : 999.50} >= float(int((isnan({in_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({in_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f))))))) && (! isnan({pf_0_1 : 999.50}))) && (! isnan(float(int((isnan({in_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({in_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
<=>
if((((((in_attr5.w > (vs_cbuf10_2.x)) && (! isnan(in_attr5.w))) && (! isnan((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((isnan(in_attr4.w) ? 0u : int(clamp(trunc(in_attr4.w), float(-2147483600.f), float(2147483600.f))))))) && (! isnan(pf_0_1))) && (! isnan(float(int((isnan(in_attr4.w) ? 0u : int(clamp(trunc(in_attr4.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
<=>if(b_1_14...)
*/
	{
		return;
	} 
	f_0_6 = in_attr7.x ;	// 0.5484
	f_1_4 = utof(vs_cbuf10[2 ].w  );	// 1.00
	pf_1_1 = pf_0_1 + f_1_4;	/* 1000.50  <=>  ({pf_0_1 : 999.50} + {(vs_cbuf10_2.w) : 1.00})
<=>
(pf_0_1 + (vs_cbuf10_2.w))
<=>
{pf_1_1 = (pf_0_1 + 
	{f_1_4 = (vs_cbuf10_2.w)
	})
}
*/
	u_2_6 = ftou(pf_1_1 );	// 1148854272
	f_1_5 = in_attr6.x ;	// 720.00
	u_4_4 = ftou(f_1_5 );	// 1144258560
	f_2_1 = utof(vs_cbuf9[15 ].x  );	// 1.00
	b_0_4 = f_2_1 == 1.f && ! isnan(f_2_1 ) && ! isnan(1.f );	// True
	f_2_2 = in_attr6.z ;	// 720.00
	f_3_1 = in_attr5.x ;	// 0
	f_4_0 = in_attr5.y ;	// 0
	f_5_0 = in_attr5.z ;	// 0
	b_1_15 = b_0_4 ? (true) : (false);	// True
	u_5_4 = u_3_2;	/* 1300234212  <=>  {ftou(float(int((isnan({in_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({in_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f))))))) : 1300234212}
<=>
ftou(float(int((isnan(in_attr4.w) ? 0u : int(clamp(trunc(in_attr4.w), float(-2147483600.f), float(2147483600.f)))))))
<=>
{u_5_4 = 
	{u_3_2 = ftou(
		{f_1_2 = float(int(
			{u_3_1 = (
				{b_0_0 = isnan(
					{f_0_0 = in_attr4.w
					})
				} ? 0u : 
				{u_3_0 = int(
					{f_1_1 = clamp(
						{f_1_0 = trunc(f_0_0)
						}, float(-2147483600.f), float(2147483600.f))
					})
				})
			}))
		})
	}
}
*/
	pf_2_phi_15 = pf_2;
	u_6_phi_15 = u_6;
	u_5_phi_15 = u_5_4;
	if(b_1_15 )	/* True  <=>  if((((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! isnan({utof(vs_cbuf9[15].x) : 1.00}))) && (! isnan(1.f))) ? true : false))
<=>
if(((((utof(vs_cbuf9[15].x) == 1.f) && (! isnan(utof(vs_cbuf9[15].x)))) && (! isnan(1.f))) ? true : false))
<=>if(b_1_15...)
*/
	{
		pf_2_0 = pf_1_1 * pf_1_1;	// 1001000.00
		u_6_0 = ftou(pf_2_0 );	// 1232364164
		u_5_5 = u_6_0;	/* 1232364164  <=>  {ftou(({pf_1_1 : 1000.50} * {pf_1_1 : 1000.50})) : 1232364164}
<=>
ftou((pf_1_1 * pf_1_1))
<=>
{u_5_5 = 
	{u_6_0 = ftou(
		{pf_2_0 = (pf_1_1 * pf_1_1)
		})
	}
}
*/
		pf_2_phi_15 = pf_2_0;
		u_6_phi_15 = u_6_0;
		u_5_phi_15 = u_5_5;
	} 
	f_6_0 = in_attr4.x ;	// 0
	f_7_0 = min(0.f, f_0_6 );	// 0
	f_8_0 = in_attr6.w ;	// 1.00
	b_1_16 = b_0_4 ? (true) : (false);	// True
	u_3_3 = u_5_phi_15;	/* 1232364164  <=>  {u_5_phi_15 : 1232364164}
<=>
u_5_phi_15
<=>
{u_3_3 = u_5_phi_15
}
*/
	f_9_phi_16 = f_9;
	f_10_phi_16 = f_10;
	pf_2_phi_16 = pf_2;
	u_6_phi_16 = u_6;
	u_3_phi_16 = u_3_3;
	if(b_1_16 )	/* True  <=>  if((((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! isnan({utof(vs_cbuf9[15].x) : 1.00}))) && (! isnan(1.f))) ? true : false))
<=>
if(((((utof(vs_cbuf9[15].x) == 1.f) && (! isnan(utof(vs_cbuf9[15].x)))) && (! isnan(1.f))) ? true : false))
<=>if(b_1_16...)
*/
	{
		f_9_0 = utof(vs_cbuf9[14 ].w  );	// 0
		f_10_0 = utof(u_5_phi_15 );	// 1001000.00
		pf_2_1 = f_10_0 * f_9_0;	// 0
		u_6_1 = ftou(pf_2_1 );	// 0
		u_3_4 = u_6_1;	/* 0  <=>  {ftou(({utof(u_5_phi_15) : 1001000.00} * {utof(vs_cbuf9[14].w) : 0})) : 0}
<=>
ftou((utof(u_5_phi_15) * utof(vs_cbuf9[14].w)))
<=>
{u_3_4 = 
	{u_6_1 = ftou(
		{pf_2_1 = (
			{f_10_0 = utof(u_5_phi_15)
			} * 
			{f_9_0 = utof(vs_cbuf9[14].w)
			})
		})
	}
}
*/
		f_9_phi_16 = f_9_0;
		f_10_phi_16 = f_10_0;
		pf_2_phi_16 = pf_2_1;
		u_6_phi_16 = u_6_1;
		u_3_phi_16 = u_3_4;
	} 
	f_9_1 = in_attr4.y ;	// 0
	f_10_1 = utof(vs_cbuf9[141 ].z  );	// 1.00
	pf_2_2 = f_2_2 * f_10_1;	// 720.00
	f_2_3 = in_attr4.z ;	// 0
	f_7_1 = min(max(f_7_0, 0.0 ), 1.0 );	// 0
	f_10_2 = in_attr0.x ;	// 0
	b_1_17 = b_0_4 ? (true) : (false);	// True
	u_5_6 = u_1_0;	/* 0  <=>  0u
<=>
0u
<=>
{u_5_6 = 
	{u_1_0 = 0u
	}
}
*/
	f_11_phi_17 = f_11;
	f_12_phi_17 = f_12;
	pf_3_phi_17 = pf_3;
	u_6_phi_17 = u_6;
	u_5_phi_17 = u_5_6;
	if(b_1_17 )	/* True  <=>  if((((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! isnan({utof(vs_cbuf9[15].x) : 1.00}))) && (! isnan(1.f))) ? true : false))
<=>
if(((((utof(vs_cbuf9[15].x) == 1.f) && (! isnan(utof(vs_cbuf9[15].x)))) && (! isnan(1.f))) ? true : false))
<=>if(b_1_17...)
*/
	{
		f_11_0 = utof(vs_cbuf9[14 ].y  );	// -1
		f_12_0 = utof(u_3_phi_16 );	// 0
		f_12_1 = f_12_0 * 0.5f;	// 0
		pf_3_0 = f_12_1 * f_11_0;	// -0
		u_6_2 = ftou(pf_3_0 );	// 2147483648
		u_5_7 = u_6_2;	/* 2147483648  <=>  {ftou((({utof(u_3_phi_16) : 0} * 0.5f) * {utof(vs_cbuf9[14].y) : -1})) : 2147483648}
<=>
ftou(((utof(u_3_phi_16) * 0.5f) * utof(vs_cbuf9[14].y)))
<=>
{u_5_7 = 
	{u_6_2 = ftou(
		{pf_3_0 = (
			{f_12_1 = (
				{f_12_0 = utof(u_3_phi_16)
				} * 0.5f)
			} * 
			{f_11_0 = utof(vs_cbuf9[14].y)
			})
		})
	}
}
*/
		f_11_phi_17 = f_11_0;
		f_12_phi_17 = f_12_1;
		pf_3_phi_17 = pf_3_0;
		u_6_phi_17 = u_6_2;
		u_5_phi_17 = u_5_7;
	} 
	f_11_1 = in_attr6.y ;	// 720.00
	b_1_18 = b_0_4 ? (true) : (false);	// True
	u_1_1 = u_0_0;	/* 0  <=>  0u
<=>
0u
<=>
{u_1_1 = 
	{u_0_0 = 0u
	}
}
*/
	f_12_phi_18 = f_12;
	f_13_phi_18 = f_13;
	pf_3_phi_18 = pf_3;
	u_6_phi_18 = u_6;
	u_1_phi_18 = u_1_1;
	if(b_1_18 )	/* True  <=>  if((((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! isnan({utof(vs_cbuf9[15].x) : 1.00}))) && (! isnan(1.f))) ? true : false))
<=>
if(((((utof(vs_cbuf9[15].x) == 1.f) && (! isnan(utof(vs_cbuf9[15].x)))) && (! isnan(1.f))) ? true : false))
<=>if(b_1_18...)
*/
	{
		f_12_2 = utof(vs_cbuf9[14 ].z  );	// 0
		f_13_0 = utof(u_3_phi_16 );	// 0
		f_13_1 = f_13_0 * 0.5f;	// 0
		pf_3_1 = f_13_1 * f_12_2;	// 0
		u_6_3 = ftou(pf_3_1 );	// 0
		u_1_2 = u_6_3;	/* 0  <=>  {ftou((({utof(u_3_phi_16) : 0} * 0.5f) * {utof(vs_cbuf9[14].z) : 0})) : 0}
<=>
ftou(((utof(u_3_phi_16) * 0.5f) * utof(vs_cbuf9[14].z)))
<=>
{u_1_2 = 
	{u_6_3 = ftou(
		{pf_3_1 = (
			{f_13_1 = (
				{f_13_0 = utof(u_3_phi_16)
				} * 0.5f)
			} * 
			{f_12_2 = utof(vs_cbuf9[14].z)
			})
		})
	}
}
*/
		f_12_phi_18 = f_12_2;
		f_13_phi_18 = f_13_1;
		pf_3_phi_18 = pf_3_1;
		u_6_phi_18 = u_6_3;
		u_1_phi_18 = u_1_2;
	} 
	f_12_3 = in_attr0.y ;	// 0
	pf_3_2 = f_7_1 + f_1_5;	// 720.00
	f_1_6 = in_attr0.z ;	// 0.001
	b_1_19 = b_0_4 ? (true) : (false);	// True
	u_0_1 = u_4_4;	/* 1144258560  <=>  {ftou(in_attr6.x) : 1144258560}
<=>
ftou(in_attr6.x)
<=>
{u_0_1 = 
	{u_4_4 = ftou(
		{f_1_5 = in_attr6.x
		})
	}
}
*/
	f_7_phi_19 = f_7_1;
	f_13_phi_19 = f_13;
	pf_4_phi_19 = pf_4;
	u_3_phi_19 = u_3_phi_16;
	u_0_phi_19 = u_0_1;
	if(b_1_19 )	/* True  <=>  if((((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! isnan({utof(vs_cbuf9[15].x) : 1.00}))) && (! isnan(1.f))) ? true : false))
<=>
if(((((utof(vs_cbuf9[15].x) == 1.f) && (! isnan(utof(vs_cbuf9[15].x)))) && (! isnan(1.f))) ? true : false))
<=>if(b_1_19...)
*/
	{
		f_7_2 = utof(vs_cbuf9[14 ].x  );	// 0
		f_13_2 = utof(u_3_phi_16 );	// 0
		f_13_3 = f_13_2 * 0.5f;	// 0
		pf_4_0 = f_13_3 * f_7_2;	// 0
		u_3_5 = ftou(pf_4_0 );	// 0
		u_0_2 = u_3_5;	/* 0  <=>  {ftou((({utof(u_3_phi_16) : 0} * 0.5f) * {utof(vs_cbuf9[14].x) : 0})) : 0}
<=>
ftou(((utof(u_3_phi_16) * 0.5f) * utof(vs_cbuf9[14].x)))
<=>
{u_0_2 = 
	{u_3_5 = ftou(
		{pf_4_0 = (
			{f_13_3 = (
				{f_13_2 = utof(u_3_phi_16)
				} * 0.5f)
			} * 
			{f_7_2 = utof(vs_cbuf9[14].x)
			})
		})
	}
}
*/
		f_7_phi_19 = f_7_2;
		f_13_phi_19 = f_13_3;
		pf_4_phi_19 = pf_4_0;
		u_3_phi_19 = u_3_5;
		u_0_phi_19 = u_0_2;
	} 
	b_1_20 = ! b_0_4;	// False
	b_2_0 = b_1_20 ? (true) : (false);	// False
	u_3_6 = u_1_phi_18;	/* 0  <=>  {u_1_phi_18 : 0}
<=>
u_1_phi_18
<=>
{u_3_6 = u_1_phi_18
}
*/
	u_4_5 = u_5_phi_17;	/* 2147483648  <=>  {u_5_phi_17 : 2147483648}
<=>
u_5_phi_17
<=>
{u_4_5 = u_5_phi_17
}
*/
	u_6_4 = u_0_phi_19;	/* 0  <=>  {u_0_phi_19 : 0}
<=>
u_0_phi_19
<=>
{u_6_4 = u_0_phi_19
}
*/
	f_7_phi_20 = f_7_phi_19;
	pf_4_phi_20 = pf_4;
	f_13_phi_20 = f_13;
	f_14_phi_20 = f_14;
	pf_5_phi_20 = pf_5;
	u_7_phi_20 = u_7;
	u_8_phi_20 = u_8;
	u_9_phi_20 = u_9;
	u_3_phi_20 = u_3_6;
	u_4_phi_20 = u_4_5;
	u_6_phi_20 = u_6_4;
	if(b_2_0 )	/* False  <=>  if(((! ((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! isnan({utof(vs_cbuf9[15].x) : 1.00}))) && (! isnan(1.f)))) ? true : false))
<=>
if(((! (((utof(vs_cbuf9[15].x) == 1.f) && (! isnan(utof(vs_cbuf9[15].x)))) && (! isnan(1.f)))) ? true : false))
<=>if(b_2_0...)
*/
	{
		f_7_3 = utof(vs_cbuf9[15 ].x  );	// 1.00
		f_7_4 = 0.f - (f_7_3 );	// -1
		pf_4_1 = f_7_4 + 1.f;	// 0
		f_7_5 = utof(vs_cbuf9[15 ].x  );	// 1.00
		f_7_6 = abs(f_7_5 );	// 1.00
		f_7_7 = log2(f_7_6 );	// 0
		f_13_4 = utof(vs_cbuf9[15 ].x  );	// 1.00
		f_13_5 = log2(f_13_4 );	// 0
		f_14_0 = (1.0f ) / pf_4_1;	// ∞
		pf_4_2 = f_7_7 * pf_1_1;	// 0
		f_7_8 = (1.0f ) / f_13_5;	// ∞
		f_13_6 = exp2(pf_4_2 );	/* 1.00  <=>  exp2((log2(abs({utof(vs_cbuf9[15].x) : 1.00})) * {pf_1_1 : 1000.50}))
<=>
exp2((log2(abs(utof(vs_cbuf9[15].x))) * pf_1_1))
<=>
{f_13_6 = exp2(
	{pf_4_2 = (
		{f_7_7 = log2(
			{f_7_6 = abs(
				{f_7_5 = utof(vs_cbuf9[15].x)
				})
			})
		} * pf_1_1)
	})
}
*/
		pf_4_3 = f_7_8 * 1.442695f;	/* ∞  <=>  ((1.0f / log2({utof(vs_cbuf9[15].x) : 1.00})) * 1.442695f)
<=>
((1.0f / log2(utof(vs_cbuf9[15].x))) * 1.442695f)
<=>
{pf_4_3 = (
	{f_7_8 = (1.0f / 
		{f_13_5 = log2(
			{f_13_4 = utof(vs_cbuf9[15].x)
			})
		})
	} * 1.442695f)
}
*/
		f_7_9 = 0.f - (pf_4_3 );	// -∞
		pf_4_4 = fma(pf_4_3, f_13_6, f_7_9 );	// NaN
		f_7_10 = 0.f - (pf_4_4 );	// NaN
		pf_4_5 = f_7_10 + pf_1_1;	// NaN
		pf_4_6 = pf_4_5 * f_14_0;	// NaN
		f_7_11 = utof(vs_cbuf9[14 ].w  );	// 0
		pf_4_7 = pf_4_6 * f_7_11;	// NaN
		f_7_12 = utof(vs_cbuf9[14 ].x  );	// 0
		pf_5_0 = pf_4_7 * f_7_12;	// NaN
		u_7_0 = ftou(pf_5_0 );	// 4290772992
		f_7_13 = utof(vs_cbuf9[14 ].y  );	// -1
		pf_5_1 = pf_4_7 * f_7_13;	// NaN
		u_8_0 = ftou(pf_5_1 );	// 4290772992
		f_7_14 = utof(vs_cbuf9[14 ].z  );	// 0
		pf_4_8 = pf_4_7 * f_7_14;	// NaN
		u_9_0 = ftou(pf_4_8 );	// 4290772992
		u_3_7 = u_9_0;	/* 4290772992  <=>  {ftou((((((0.f - (({pf_4_3 : ∞} * {f_13_6 : 1.00}) + (0.f - {pf_4_3 : ∞}))) + {pf_1_1 : 1000.50}) * (1.0f / ((0.f - {utof(vs_cbuf9[15].x) : 1.00}) + 1.f))) * {utof(vs_cbuf9[14].w) : 0}) * {utof(vs_cbuf9[14].z) : 0})) : 4290772992}
<=>
ftou((((((0.f - ((pf_4_3 * f_13_6) + (0.f - pf_4_3))) + pf_1_1) * (1.0f / ((0.f - utof(vs_cbuf9[15].x)) + 1.f))) * utof(vs_cbuf9[14].w)) * utof(vs_cbuf9[14].z)))
<=>
{u_3_7 = 
	{u_9_0 = ftou(
		{pf_4_8 = (
			{pf_4_7 = (
				{pf_4_6 = (
					{pf_4_5 = (
						{f_7_10 = (0.f - 
							{pf_4_4 = ((pf_4_3 * f_13_6) + 
								{f_7_9 = (0.f - pf_4_3)
								})
							})
						} + pf_1_1)
					} * 
					{f_14_0 = (1.0f / 
						{pf_4_1 = (
							{f_7_4 = (0.f - 
								{f_7_3 = utof(vs_cbuf9[15].x)
								})
							} + 1.f)
						})
					})
				} * 
				{f_7_11 = utof(vs_cbuf9[14].w)
				})
			} * 
			{f_7_14 = utof(vs_cbuf9[14].z)
			})
		})
	}
}
*/
		u_4_6 = u_8_0;	/* 4290772992  <=>  {ftou((((((0.f - (({pf_4_3 : ∞} * {f_13_6 : 1.00}) + (0.f - {pf_4_3 : ∞}))) + {pf_1_1 : 1000.50}) * (1.0f / ((0.f - {utof(vs_cbuf9[15].x) : 1.00}) + 1.f))) * {utof(vs_cbuf9[14].w) : 0}) * {utof(vs_cbuf9[14].y) : -1})) : 4290772992}
<=>
ftou((((((0.f - ((pf_4_3 * f_13_6) + (0.f - pf_4_3))) + pf_1_1) * (1.0f / ((0.f - utof(vs_cbuf9[15].x)) + 1.f))) * utof(vs_cbuf9[14].w)) * utof(vs_cbuf9[14].y)))
<=>
{u_4_6 = 
	{u_8_0 = ftou(
		{pf_5_1 = (
			{pf_4_7 = (
				{pf_4_6 = (
					{pf_4_5 = (
						{f_7_10 = (0.f - 
							{pf_4_4 = ((pf_4_3 * f_13_6) + 
								{f_7_9 = (0.f - pf_4_3)
								})
							})
						} + pf_1_1)
					} * 
					{f_14_0 = (1.0f / 
						{pf_4_1 = (
							{f_7_4 = (0.f - 
								{f_7_3 = utof(vs_cbuf9[15].x)
								})
							} + 1.f)
						})
					})
				} * 
				{f_7_11 = utof(vs_cbuf9[14].w)
				})
			} * 
			{f_7_13 = utof(vs_cbuf9[14].y)
			})
		})
	}
}
*/
		u_6_5 = u_7_0;	/* 4290772992  <=>  {ftou((((((0.f - (({pf_4_3 : ∞} * {f_13_6 : 1.00}) + (0.f - {pf_4_3 : ∞}))) + {pf_1_1 : 1000.50}) * (1.0f / ((0.f - {utof(vs_cbuf9[15].x) : 1.00}) + 1.f))) * {utof(vs_cbuf9[14].w) : 0}) * {utof(vs_cbuf9[14].x) : 0})) : 4290772992}
<=>
ftou((((((0.f - ((pf_4_3 * f_13_6) + (0.f - pf_4_3))) + pf_1_1) * (1.0f / ((0.f - utof(vs_cbuf9[15].x)) + 1.f))) * utof(vs_cbuf9[14].w)) * utof(vs_cbuf9[14].x)))
<=>
{u_6_5 = 
	{u_7_0 = ftou(
		{pf_5_0 = (
			{pf_4_7 = (
				{pf_4_6 = (
					{pf_4_5 = (
						{f_7_10 = (0.f - 
							{pf_4_4 = ((pf_4_3 * f_13_6) + 
								{f_7_9 = (0.f - pf_4_3)
								})
							})
						} + pf_1_1)
					} * 
					{f_14_0 = (1.0f / 
						{pf_4_1 = (
							{f_7_4 = (0.f - 
								{f_7_3 = utof(vs_cbuf9[15].x)
								})
							} + 1.f)
						})
					})
				} * 
				{f_7_11 = utof(vs_cbuf9[14].w)
				})
			} * 
			{f_7_12 = utof(vs_cbuf9[14].x)
			})
		})
	}
}
*/
		f_7_phi_20 = f_7_14;
		pf_4_phi_20 = pf_4_8;
		f_13_phi_20 = f_13_6;
		f_14_phi_20 = f_14_0;
		pf_5_phi_20 = pf_5_1;
		u_7_phi_20 = u_7_0;
		u_8_phi_20 = u_8_0;
		u_9_phi_20 = u_9_0;
		u_3_phi_20 = u_3_7;
		u_4_phi_20 = u_4_6;
		u_6_phi_20 = u_6_5;
	} 
	b_0_5 = ! b_0_4;	// False
	b_1_21 = b_0_5 ? (true) : (false);	// False
	u_0_3 = u_2_6;	/* 1148854272  <=>  {ftou(pf_1_1) : 1148854272}
<=>
ftou(pf_1_1)
<=>
{u_0_3 = 
	{u_2_6 = ftou(pf_1_1)
	}
}
*/
	f_7_phi_21 = f_7_phi_20;
	pf_4_phi_21 = pf_4;
	f_13_phi_21 = f_13;
	pf_1_phi_21 = pf_1_1;
	f_14_phi_21 = f_14;
	u_1_phi_21 = u_1_phi_18;
	u_0_phi_21 = u_0_3;
	if(b_1_21 )	/* False  <=>  if(((! ((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! isnan({utof(vs_cbuf9[15].x) : 1.00}))) && (! isnan(1.f)))) ? true : false))
<=>
if(((! (((utof(vs_cbuf9[15].x) == 1.f) && (! isnan(utof(vs_cbuf9[15].x)))) && (! isnan(1.f)))) ? true : false))
<=>if(b_1_21...)
*/
	{
		f_7_15 = utof(vs_cbuf9[15 ].x  );	// 1.00
		f_7_16 = 0.f - (f_7_15 );	// -1
		pf_4_9 = f_7_16 + 1.f;	// 0
		f_7_17 = utof(vs_cbuf9[15 ].x  );	// 1.00
		f_7_18 = abs(f_7_17 );	// 1.00
		f_7_19 = log2(f_7_18 );	// 0
		f_13_7 = (1.0f ) / pf_4_9;	/* ∞  <=>  (1.0f / ((0.f - {utof(vs_cbuf9[15].x) : 1.00}) + 1.f))
<=>
(1.0f / ((0.f - utof(vs_cbuf9[15].x)) + 1.f))
<=>
{f_13_7 = (1.0f / 
	{pf_4_9 = (
		{f_7_16 = (0.f - 
			{f_7_15 = utof(vs_cbuf9[15].x)
			})
		} + 1.f)
	})
}
*/
		pf_1_2 = f_7_19 * pf_1_1;	// 0
		f_7_20 = exp2(pf_1_2 );	/* 1.00  <=>  exp2((log2(abs({utof(vs_cbuf9[15].x) : 1.00})) * {pf_1_1 : 1000.50}))
<=>
exp2((log2(abs(utof(vs_cbuf9[15].x))) * pf_1_1))
<=>
{f_7_20 = exp2(
	{pf_1_2 = (
		{f_7_19 = log2(
			{f_7_18 = abs(
				{f_7_17 = utof(vs_cbuf9[15].x)
				})
			})
		} * pf_1_1)
	})
}
*/
		f_14_1 = 0.f - (f_13_7 );	// -∞
		pf_1_3 = fma(f_7_20, f_14_1, f_13_7 );	// NaN
		u_1_3 = ftou(pf_1_3 );	// 4290772992
		u_0_4 = u_1_3;	/* 4290772992  <=>  {ftou((({f_7_20 : 1.00} * (0.f - {f_13_7 : ∞})) + {f_13_7 : ∞})) : 4290772992}
<=>
ftou(((f_7_20 * (0.f - f_13_7)) + f_13_7))
<=>
{u_0_4 = 
	{u_1_3 = ftou(
		{pf_1_3 = ((f_7_20 * 
			{f_14_1 = (0.f - f_13_7)
			}) + f_13_7)
		})
	}
}
*/
		f_7_phi_21 = f_7_20;
		pf_4_phi_21 = pf_4_9;
		f_13_phi_21 = f_13_7;
		pf_1_phi_21 = pf_1_3;
		f_14_phi_21 = f_14_1;
		u_1_phi_21 = u_1_3;
		u_0_phi_21 = u_0_4;
	} 
	f_7_21 = utof(vs_cbuf9[141 ].x  );	// 1.00
	pf_1_4 = pf_3_2 * f_7_21;	// 720.00
	f_7_22 = utof(u_6_phi_20 );	// 0
	f_13_8 = utof(u_0_phi_21 );	// 1000.50
	pf_3_3 = fma(f_13_8, f_3_1, f_7_22 );	// 0
	f_3_2 = utof(vs_cbuf9[16 ].z  );	// 0
	pf_4_10 = fma(0.5f, f_3_2, f_1_6 );	// 0.001
	f_3_3 = utof(vs_cbuf10[3 ].w  );	// 1.00
	pf_2_3 = pf_2_2 * f_3_3;	// 720.00
	f_3_4 = utof(vs_cbuf9[16 ].x  );	// 0
	pf_5_2 = fma(0.5f, f_3_4, f_10_2 );	// 0
	f_3_5 = utof(u_4_phi_20 );	// -0
	f_7_23 = utof(u_0_phi_21 );	// 1000.50
	pf_6_0 = fma(f_7_23, f_4_0, f_3_5 );	// 0
	f_3_6 = utof(vs_cbuf10[3 ].y  );	// 1.00
	pf_1_5 = pf_1_4 * f_3_6;	// 720.00
	pf_3_4 = fma(pf_3_3, f_8_0, f_6_0 );	/* 0  <=>  (((({utof(u_0_phi_21) : 1000.50} * {in_attr5.x : 0}) + {utof(u_6_phi_20) : 0}) * {in_attr6.w : 1.00}) + {in_attr4.x : 0})
<=>
((((utof(u_0_phi_21) * in_attr5.x) + utof(u_6_phi_20)) * in_attr6.w) + in_attr4.x)
<=>
{pf_3_4 = ((
	{pf_3_3 = ((
		{f_13_8 = utof(u_0_phi_21)
		} * 
		{f_3_1 = in_attr5.x
		}) + 
		{f_7_22 = utof(u_6_phi_20)
		})
	} * 
	{f_8_0 = in_attr6.w
	}) + 
	{f_6_0 = in_attr4.x
	})
}
*/
	f_3_7 = utof(u_3_phi_20 );	// 0
	f_4_1 = utof(u_0_phi_21 );	// 1000.50
	pf_7_0 = fma(f_4_1, f_5_0, f_3_7 );	// 0
	pf_2_4 = pf_2_3 * pf_4_10;	/* 0.72  <=>  ((({in_attr6.z : 720.00} * {utof(vs_cbuf9[141].z) : 1.00}) * {(vs_cbuf10_3.w) : 1.00}) * ((0.5f * {utof(vs_cbuf9[16].z) : 0}) + {in_attr0.z : 0.001}))
<=>
(((in_attr6.z * utof(vs_cbuf9[141].z)) * (vs_cbuf10_3.w)) * ((0.5f * utof(vs_cbuf9[16].z)) + in_attr0.z))
<=>
{pf_2_4 = (
	{pf_2_3 = (
		{pf_2_2 = (
			{f_2_2 = in_attr6.z
			} * 
			{f_10_1 = utof(vs_cbuf9[141].z)
			})
		} * 
		{f_3_3 = (vs_cbuf10_3.w)
		})
	} * 
	{pf_4_10 = ((0.5f * 
		{f_3_2 = utof(vs_cbuf9[16].z)
		}) + 
		{f_1_6 = in_attr0.z
		})
	})
}
*/
	f_3_8 = utof(vs_cbuf9[141 ].y  );	// 1.00
	pf_4_11 = f_11_1 * f_3_8;	// 720.00
	pf_6_1 = fma(pf_6_0, f_8_0, f_9_1 );	/* 0  <=>  (((({utof(u_0_phi_21) : 1000.50} * {in_attr5.y : 0}) + {utof(u_4_phi_20) : -0}) * {in_attr6.w : 1.00}) + {in_attr4.y : 0})
<=>
((((utof(u_0_phi_21) * in_attr5.y) + utof(u_4_phi_20)) * in_attr6.w) + in_attr4.y)
<=>
{pf_6_1 = ((
	{pf_6_0 = ((
		{f_7_23 = utof(u_0_phi_21)
		} * 
		{f_4_0 = in_attr5.y
		}) + 
		{f_3_5 = utof(u_4_phi_20)
		})
	} * 
	{f_8_0 = in_attr6.w
	}) + 
	{f_9_1 = in_attr4.y
	})
}
*/
	pf_1_6 = pf_1_5 * pf_5_2;	/* 0  <=>  ((((clamp(min(0.f, {in_attr7.x : 0.5484}), 0.0, 1.0) + {in_attr6.x : 720.00}) * {utof(vs_cbuf9[141].x) : 1.00}) * {(vs_cbuf10_3.y) : 1.00}) * ((0.5f * {utof(vs_cbuf9[16].x) : 0}) + {in_attr0.x : 0}))
<=>
((((clamp(min(0.f, in_attr7.x), 0.0, 1.0) + in_attr6.x) * utof(vs_cbuf9[141].x)) * (vs_cbuf10_3.y)) * ((0.5f * utof(vs_cbuf9[16].x)) + in_attr0.x))
<=>
{pf_1_6 = (
	{pf_1_5 = (
		{pf_1_4 = (
			{pf_3_2 = (
				{f_7_1 = clamp(
					{f_7_0 = min(0.f, 
						{f_0_6 = in_attr7.x
						})
					}, 0.0, 1.0)
				} + 
				{f_1_5 = in_attr6.x
				})
			} * 
			{f_7_21 = utof(vs_cbuf9[141].x)
			})
		} * 
		{f_3_6 = (vs_cbuf10_3.y)
		})
	} * 
	{pf_5_2 = ((0.5f * 
		{f_3_4 = utof(vs_cbuf9[16].x)
		}) + 
		{f_10_2 = in_attr0.x
		})
	})
}
*/
	f_3_9 = utof(vs_cbuf10[4 ].x  );	// 1.00
	pf_5_3 = pf_3_4 * f_3_9;	// 0
	f_3_10 = utof(vs_cbuf9[16 ].y  );	// 0
	pf_8_0 = fma(0.5f, f_3_10, f_12_3 );	// 0
	pf_7_1 = fma(pf_7_0, f_8_0, f_2_3 );	/* 0  <=>  (((({utof(u_0_phi_21) : 1000.50} * {in_attr5.z : 0}) + {utof(u_3_phi_20) : 0}) * {in_attr6.w : 1.00}) + {in_attr4.z : 0})
<=>
((((utof(u_0_phi_21) * in_attr5.z) + utof(u_3_phi_20)) * in_attr6.w) + in_attr4.z)
<=>
{pf_7_1 = ((
	{pf_7_0 = ((
		{f_4_1 = utof(u_0_phi_21)
		} * 
		{f_5_0 = in_attr5.z
		}) + 
		{f_3_7 = utof(u_3_phi_20)
		})
	} * 
	{f_8_0 = in_attr6.w
	}) + 
	{f_2_3 = in_attr4.z
	})
}
*/
	f_2_4 = utof(vs_cbuf10[3 ].z  );	// 1.00
	pf_4_12 = pf_4_11 * f_2_4;	// 720.00
	f_2_5 = utof(vs_cbuf10[5 ].x  );	// 0
	pf_9_0 = pf_3_4 * f_2_5;	// 0
	f_2_6 = utof(vs_cbuf10[8 ].x  );	// 1.00
	pf_10_0 = pf_1_6 * f_2_6;	// 0
	f_2_7 = utof(vs_cbuf10[4 ].y  );	// 0
	pf_5_4 = fma(pf_6_1, f_2_7, pf_5_3 );	// 0
	f_2_8 = utof(vs_cbuf10[6 ].x  );	// 0
	pf_3_5 = pf_3_4 * f_2_8;	// 0
	pf_4_13 = pf_4_12 * pf_8_0;	/* 0  <=>  ((({in_attr6.y : 720.00} * {utof(vs_cbuf9[141].y) : 1.00}) * {(vs_cbuf10_3.z) : 1.00}) * ((0.5f * {utof(vs_cbuf9[16].y) : 0}) + {in_attr0.y : 0}))
<=>
(((in_attr6.y * utof(vs_cbuf9[141].y)) * (vs_cbuf10_3.z)) * ((0.5f * utof(vs_cbuf9[16].y)) + in_attr0.y))
<=>
{pf_4_13 = (
	{pf_4_12 = (
		{pf_4_11 = (
			{f_11_1 = in_attr6.y
			} * 
			{f_3_8 = utof(vs_cbuf9[141].y)
			})
		} * 
		{f_2_4 = (vs_cbuf10_3.z)
		})
	} * 
	{pf_8_0 = ((0.5f * 
		{f_3_10 = utof(vs_cbuf9[16].y)
		}) + 
		{f_12_3 = in_attr0.y
		})
	})
}
*/
	f_2_9 = utof(vs_cbuf10[9 ].x  );	// 0
	pf_8_1 = pf_1_6 * f_2_9;	// 0
	f_2_10 = utof(vs_cbuf10[8 ].y  );	// 0
	pf_10_1 = fma(pf_2_4, f_2_10, pf_10_0 );	// 0
	f_2_11 = utof(vs_cbuf10[5 ].y  );	// 1.00
	pf_9_1 = fma(pf_6_1, f_2_11, pf_9_0 );	// 0
	f_2_12 = utof(vs_cbuf10[4 ].z  );	// 0
	pf_5_5 = fma(pf_7_1, f_2_12, pf_5_4 );	// 0
	f_2_13 = utof(vs_cbuf10[6 ].y  );	// 0
	pf_3_6 = fma(pf_6_1, f_2_13, pf_3_5 );	// 0
	f_2_14 = utof(vs_cbuf10[10 ].x  );	// 0
	pf_1_7 = pf_1_6 * f_2_14;	// 0
	f_2_15 = utof(vs_cbuf10[9 ].y  );	// 1.00
	pf_6_2 = fma(pf_2_4, f_2_15, pf_8_1 );	// 0.72
	f_2_16 = utof(vs_cbuf10[8 ].z  );	// 0
	f_2_17 = 0.f - (f_2_16 );	// 0
	pf_8_2 = fma(pf_4_13, f_2_17, pf_10_1 );	// 0
	f_2_18 = utof(vs_cbuf10[5 ].z  );	// 0
	pf_9_2 = fma(pf_7_1, f_2_18, pf_9_1 );	// 0
	f_2_19 = utof(vs_cbuf10[4 ].w  );	// -1134.2701
	pf_5_6 = pf_5_5 + f_2_19;	// -1134.2701
	f_2_20 = utof(vs_cbuf10[6 ].z  );	// 1.00
	pf_3_7 = fma(pf_7_1, f_2_20, pf_3_6 );	// 0
	f_2_21 = utof(vs_cbuf10[10 ].y  );	// 0
	pf_1_8 = fma(pf_2_4, f_2_21, pf_1_7 );	// 0
	f_2_22 = utof(vs_cbuf10[9 ].z  );	// 0
	f_2_23 = 0.f - (f_2_22 );	// 0
	pf_2_5 = fma(pf_4_13, f_2_23, pf_6_2 );	// 0.72
	f_2_24 = utof(vs_cbuf10[5 ].w  );	// 35.58958
	pf_6_3 = pf_9_2 + f_2_24;	// 35.58958
	pf_7_2 = pf_5_6 + pf_8_2;	/* -1134.2701  <=>  (((({pf_7_1 : 0} * {(vs_cbuf10_4.z) : 0}) + (({pf_6_1 : 0} * {(vs_cbuf10_4.y) : 0}) + ({pf_3_4 : 0} * {(vs_cbuf10_4.x) : 1.00}))) + {(vs_cbuf10_4.w) : -1134.2701}) + (({pf_4_13 : 0} * (0.f - {(vs_cbuf10_8.z) : 0})) + (({pf_2_4 : 0.72} * {(vs_cbuf10_8.y) : 0}) + ({pf_1_6 : 0} * {(vs_cbuf10_8.x) : 1.00}))))
<=>
((((pf_7_1 * (vs_cbuf10_4.z)) + ((pf_6_1 * (vs_cbuf10_4.y)) + (pf_3_4 * (vs_cbuf10_4.x)))) + (vs_cbuf10_4.w)) + ((pf_4_13 * (0.f - (vs_cbuf10_8.z))) + ((pf_2_4 * (vs_cbuf10_8.y)) + (pf_1_6 * (vs_cbuf10_8.x)))))
<=>
{pf_7_2 = (
	{pf_5_6 = (
		{pf_5_5 = ((pf_7_1 * 
			{f_2_12 = (vs_cbuf10_4.z)
			}) + 
			{pf_5_4 = ((pf_6_1 * 
				{f_2_7 = (vs_cbuf10_4.y)
				}) + 
				{pf_5_3 = (pf_3_4 * 
					{f_3_9 = (vs_cbuf10_4.x)
					})
				})
			})
		} + 
		{f_2_19 = (vs_cbuf10_4.w)
		})
	} + 
	{pf_8_2 = ((pf_4_13 * 
		{f_2_17 = (0.f - 
			{f_2_16 = (vs_cbuf10_8.z)
			})
		}) + 
		{pf_10_1 = ((pf_2_4 * 
			{f_2_10 = (vs_cbuf10_8.y)
			}) + 
			{pf_10_0 = (pf_1_6 * 
				{f_2_6 = (vs_cbuf10_8.x)
				})
			})
		})
	})
}
*/
	f_2_25 = utof(vs_cbuf10[6 ].w  );	// -3936.7583
	pf_3_8 = pf_3_7 + f_2_25;	// -3936.7583
	f_2_26 = utof(vs_cbuf10[10 ].z  );	// 1.00
	f_2_27 = 0.f - (f_2_26 );	// -1
	pf_1_9 = fma(pf_4_13, f_2_27, pf_1_8 );	// 0
	pf_2_6 = pf_6_3 + pf_2_5;	/* 36.30958  <=>  (((({pf_7_1 : 0} * {(vs_cbuf10_5.z) : 0}) + (({pf_6_1 : 0} * {(vs_cbuf10_5.y) : 1.00}) + ({pf_3_4 : 0} * {(vs_cbuf10_5.x) : 0}))) + {(vs_cbuf10_5.w) : 35.58958}) + (({pf_4_13 : 0} * (0.f - {(vs_cbuf10_9.z) : 0})) + (({pf_2_4 : 0.72} * {(vs_cbuf10_9.y) : 1.00}) + ({pf_1_6 : 0} * {(vs_cbuf10_9.x) : 0}))))
<=>
((((pf_7_1 * (vs_cbuf10_5.z)) + ((pf_6_1 * (vs_cbuf10_5.y)) + (pf_3_4 * (vs_cbuf10_5.x)))) + (vs_cbuf10_5.w)) + ((pf_4_13 * (0.f - (vs_cbuf10_9.z))) + ((pf_2_4 * (vs_cbuf10_9.y)) + (pf_1_6 * (vs_cbuf10_9.x)))))
<=>
{pf_2_6 = (
	{pf_6_3 = (
		{pf_9_2 = ((pf_7_1 * 
			{f_2_18 = (vs_cbuf10_5.z)
			}) + 
			{pf_9_1 = ((pf_6_1 * 
				{f_2_11 = (vs_cbuf10_5.y)
				}) + 
				{pf_9_0 = (pf_3_4 * 
					{f_2_5 = (vs_cbuf10_5.x)
					})
				})
			})
		} + 
		{f_2_24 = (vs_cbuf10_5.w)
		})
	} + 
	{pf_2_5 = ((pf_4_13 * 
		{f_2_23 = (0.f - 
			{f_2_22 = (vs_cbuf10_9.z)
			})
		}) + 
		{pf_6_2 = ((pf_2_4 * 
			{f_2_15 = (vs_cbuf10_9.y)
			}) + 
			{pf_8_1 = (pf_1_6 * 
				{f_2_9 = (vs_cbuf10_9.x)
				})
			})
		})
	})
}
*/
	f_2_28 = utof(vs_cbuf8[0 ].x  );	// -0.7425708
	pf_4_14 = pf_7_2 * f_2_28;	// 842.2759
	f_2_29 = utof(vs_cbuf8[1 ].x  );	// 0.339885
	pf_8_3 = pf_7_2 * f_2_29;	// -385.5214
	pf_1_10 = pf_3_8 + pf_1_9;	/* -3936.7583  <=>  (((({pf_7_1 : 0} * {(vs_cbuf10_6.z) : 1.00}) + (({pf_6_1 : 0} * {(vs_cbuf10_6.y) : 0}) + ({pf_3_4 : 0} * {(vs_cbuf10_6.x) : 0}))) + {(vs_cbuf10_6.w) : -3936.7583}) + (({pf_4_13 : 0} * (0.f - {(vs_cbuf10_10.z) : 1.00})) + (({pf_2_4 : 0.72} * {(vs_cbuf10_10.y) : 0}) + ({pf_1_6 : 0} * {(vs_cbuf10_10.x) : 0}))))
<=>
((((pf_7_1 * (vs_cbuf10_6.z)) + ((pf_6_1 * (vs_cbuf10_6.y)) + (pf_3_4 * (vs_cbuf10_6.x)))) + (vs_cbuf10_6.w)) + ((pf_4_13 * (0.f - (vs_cbuf10_10.z))) + ((pf_2_4 * (vs_cbuf10_10.y)) + (pf_1_6 * (vs_cbuf10_10.x)))))
<=>
{pf_1_10 = (
	{pf_3_8 = (
		{pf_3_7 = ((pf_7_1 * 
			{f_2_20 = (vs_cbuf10_6.z)
			}) + 
			{pf_3_6 = ((pf_6_1 * 
				{f_2_13 = (vs_cbuf10_6.y)
				}) + 
				{pf_3_5 = (pf_3_4 * 
					{f_2_8 = (vs_cbuf10_6.x)
					})
				})
			})
		} + 
		{f_2_25 = (vs_cbuf10_6.w)
		})
	} + 
	{pf_1_9 = ((pf_4_13 * 
		{f_2_27 = (0.f - 
			{f_2_26 = (vs_cbuf10_10.z)
			})
		}) + 
		{pf_1_8 = ((pf_2_4 * 
			{f_2_21 = (vs_cbuf10_10.y)
			}) + 
			{pf_1_7 = (pf_1_6 * 
				{f_2_14 = (vs_cbuf10_10.x)
				})
			})
		})
	})
}
*/
	f_2_30 = utof(vs_cbuf8[0 ].y  );	// 1.493044E-08
	pf_4_15 = fma(pf_2_6, f_2_30, pf_4_14 );	// 842.2759
	f_2_31 = utof(vs_cbuf8[2 ].x  );	// -0.57711935
	pf_9_3 = pf_7_2 * f_2_31;	// 654.6093
	f_2_32 = utof(vs_cbuf8[1 ].y  );	// 0.8616711
	pf_8_4 = fma(pf_2_6, f_2_32, pf_8_3 );	// -354.23447
	f_2_33 = utof(vs_cbuf8[3 ].x  );	// 0
	pf_10_2 = pf_7_2 * f_2_33;	// -0
	f_2_34 = utof(vs_cbuf8[0 ].z  );	// 0.6697676
	pf_4_16 = fma(pf_1_10, f_2_34, pf_4_15 );	// -1794.4374
	f_2_35 = utof(vs_cbuf8[2 ].y  );	// 0.5074672
	pf_9_4 = fma(pf_2_6, f_2_35, pf_9_3 );	// 673.0352
	f_2_36 = utof(vs_cbuf8[1 ].z  );	// 0.3768303
	pf_8_5 = fma(pf_1_10, f_2_36, pf_8_4 );	// -1837.7244
	f_2_37 = utof(vs_cbuf8[3 ].y  );	// 0
	pf_10_3 = fma(pf_2_6, f_2_37, pf_10_2 );	// 0
	f_2_38 = utof(vs_cbuf8[0 ].w  );	// 1075.086
	pf_4_17 = pf_4_16 + f_2_38;	/* -719.3513  <=>  ((({pf_1_10 : -3936.7583} * {(view_proj[0].z) : 0.6697676}) + (({pf_2_6 : 36.30958} * {(view_proj[0].y) : 1.493044E-08}) + ({pf_7_2 : -1134.2701} * {(view_proj[0].x) : -0.7425708}))) + {(view_proj[0].w) : 1075.086})
<=>
(((pf_1_10 * (view_proj[0].z)) + ((pf_2_6 * (view_proj[0].y)) + (pf_7_2 * (view_proj[0].x)))) + (view_proj[0].w))
<=>
{pf_4_17 = (
	{pf_4_16 = ((pf_1_10 * 
		{f_2_34 = (view_proj[0].z)
		}) + 
		{pf_4_15 = ((pf_2_6 * 
			{f_2_30 = (view_proj[0].y)
			}) + 
			{pf_4_14 = (pf_7_2 * 
				{f_2_28 = (view_proj[0].x)
				})
			})
		})
	} + 
	{f_2_38 = (view_proj[0].w)
	})
}
*/
	f_2_39 = utof(vs_cbuf8[2 ].z  );	// -0.6398518
	pf_9_5 = fma(pf_1_10, f_2_39, pf_9_4 );	// 3191.977
	f_2_40 = utof(vs_cbuf8[1 ].w  );	// 1743.908
	pf_8_6 = pf_8_5 + f_2_40;	/* -93.81641  <=>  ((({pf_1_10 : -3936.7583} * {(view_proj[1].z) : 0.3768303}) + (({pf_2_6 : 36.30958} * {(view_proj[1].y) : 0.8616711}) + ({pf_7_2 : -1134.2701} * {(view_proj[1].x) : 0.339885}))) + {(view_proj[1].w) : 1743.908})
<=>
(((pf_1_10 * (view_proj[1].z)) + ((pf_2_6 * (view_proj[1].y)) + (pf_7_2 * (view_proj[1].x)))) + (view_proj[1].w))
<=>
{pf_8_6 = (
	{pf_8_5 = ((pf_1_10 * 
		{f_2_36 = (view_proj[1].z)
		}) + 
		{pf_8_4 = ((pf_2_6 * 
			{f_2_32 = (view_proj[1].y)
			}) + 
			{pf_8_3 = (pf_7_2 * 
				{f_2_29 = (view_proj[1].x)
				})
			})
		})
	} + 
	{f_2_40 = (view_proj[1].w)
	})
}
*/
	f_2_41 = utof(vs_cbuf8[3 ].z  );	// 0
	pf_10_4 = fma(pf_1_10, f_2_41, pf_10_3 );	// 0
	f_2_42 = utof(vs_cbuf8[5 ].x  );	// 0
	pf_11_0 = pf_4_17 * f_2_42;	// -0
	f_2_43 = utof(vs_cbuf8[2 ].w  );	// -3681.8398
	pf_9_6 = pf_9_5 + f_2_43;	/* -489.8628  <=>  ((({pf_1_10 : -3936.7583} * {(view_proj[2].z) : -0.6398518}) + (({pf_2_6 : 36.30958} * {(view_proj[2].y) : 0.5074672}) + ({pf_7_2 : -1134.2701} * {(view_proj[2].x) : -0.57711935}))) + {(view_proj[2].w) : -3681.8398})
<=>
(((pf_1_10 * (view_proj[2].z)) + ((pf_2_6 * (view_proj[2].y)) + (pf_7_2 * (view_proj[2].x)))) + (view_proj[2].w))
<=>
{pf_9_6 = (
	{pf_9_5 = ((pf_1_10 * 
		{f_2_39 = (view_proj[2].z)
		}) + 
		{pf_9_4 = ((pf_2_6 * 
			{f_2_35 = (view_proj[2].y)
			}) + 
			{pf_9_3 = (pf_7_2 * 
				{f_2_31 = (view_proj[2].x)
				})
			})
		})
	} + 
	{f_2_43 = (view_proj[2].w)
	})
}
*/
	f_2_44 = utof(vs_cbuf8[4 ].x  );	// 1.206285
	pf_12_0 = pf_4_17 * f_2_44;	// -867.7427
	f_2_45 = utof(vs_cbuf8[3 ].w  );	// 1.00
	pf_10_5 = pf_10_4 + f_2_45;	/* 1.00  <=>  ((({pf_1_10 : -3936.7583} * {(view_proj[3].z) : 0}) + (({pf_2_6 : 36.30958} * {(view_proj[3].y) : 0}) + ({pf_7_2 : -1134.2701} * {(view_proj[3].x) : 0}))) + {(view_proj[3].w) : 1.00})
<=>
(((pf_1_10 * (view_proj[3].z)) + ((pf_2_6 * (view_proj[3].y)) + (pf_7_2 * (view_proj[3].x)))) + (view_proj[3].w))
<=>
{pf_10_5 = (
	{pf_10_4 = ((pf_1_10 * 
		{f_2_41 = (view_proj[3].z)
		}) + 
		{pf_10_3 = ((pf_2_6 * 
			{f_2_37 = (view_proj[3].y)
			}) + 
			{pf_10_2 = (pf_7_2 * 
				{f_2_33 = (view_proj[3].x)
				})
			})
		})
	} + 
	{f_2_45 = (view_proj[3].w)
	})
}
*/
	f_2_46 = utof(vs_cbuf8[6 ].x  );	// 0
	pf_13_0 = pf_4_17 * f_2_46;	// -0
	f_2_47 = utof(vs_cbuf8[7 ].x  );	// 0
	pf_4_18 = pf_4_17 * f_2_47;	// -0
	f_2_48 = utof(vs_cbuf8[5 ].y  );	// 2.144507
	pf_11_1 = fma(pf_8_6, f_2_48, pf_11_0 );	// -201.18993
	f_2_49 = utof(vs_cbuf8[4 ].y  );	// 0
	pf_12_1 = fma(pf_8_6, f_2_49, pf_12_0 );	// -867.7427
	f_2_50 = utof(vs_cbuf8[6 ].y  );	// 0
	pf_13_1 = fma(pf_8_6, f_2_50, pf_13_0 );	// -0
	f_2_51 = utof(vs_cbuf8[7 ].y  );	// 0
	pf_4_19 = fma(pf_8_6, f_2_51, pf_4_18 );	// -0
	f_2_52 = utof(vs_cbuf8[5 ].z  );	// 0
	pf_8_7 = fma(pf_9_6, f_2_52, pf_11_1 );	// -201.18993
	f_2_53 = utof(vs_cbuf8[4 ].z  );	// 0
	pf_11_2 = fma(pf_9_6, f_2_53, pf_12_1 );	// -867.7427
	f_2_54 = utof(vs_cbuf8[6 ].z  );	// -1.000008
	pf_12_2 = fma(pf_9_6, f_2_54, pf_13_1 );	// 489.8667
	f_2_55 = utof(vs_cbuf8[7 ].z  );	// -1
	pf_4_20 = fma(pf_9_6, f_2_55, pf_4_19 );	// 489.8628
	f_2_56 = utof(vs_cbuf8[5 ].w  );	// 0
	pf_8_8 = fma(pf_10_5, f_2_56, pf_8_7 );	/* -201.18993  <=>  (({pf_10_5 : 1.00} * {(view_proj[5].w) : 0}) + (({pf_9_6 : -489.8628} * {(view_proj[5].z) : 0}) + (({pf_8_6 : -93.81641} * {(view_proj[5].y) : 2.144507}) + ({pf_4_17 : -719.3513} * {(view_proj[5].x) : 0}))))
<=>
((pf_10_5 * (view_proj[5].w)) + ((pf_9_6 * (view_proj[5].z)) + ((pf_8_6 * (view_proj[5].y)) + (pf_4_17 * (view_proj[5].x)))))
<=>
{pf_8_8 = ((pf_10_5 * 
	{f_2_56 = (view_proj[5].w)
	}) + 
	{pf_8_7 = ((pf_9_6 * 
		{f_2_52 = (view_proj[5].z)
		}) + 
		{pf_11_1 = ((pf_8_6 * 
			{f_2_48 = (view_proj[5].y)
			}) + 
			{pf_11_0 = (pf_4_17 * 
				{f_2_42 = (view_proj[5].x)
				})
			})
		})
	})
}
*/
	f_2_57 = utof(vs_cbuf8[4 ].w  );	// 0
	pf_9_7 = fma(pf_10_5, f_2_57, pf_11_2 );	// -867.7427
	f_2_58 = utof(vs_cbuf8[6 ].w  );	// -0.2000008
	pf_11_3 = fma(pf_10_5, f_2_58, pf_12_2 );	/* 489.6667  <=>  (({pf_10_5 : 1.00} * {(view_proj[6].w) : -0.2000008}) + (({pf_9_6 : -489.8628} * {(view_proj[6].z) : -1.000008}) + (({pf_8_6 : -93.81641} * {(view_proj[6].y) : 0}) + ({pf_4_17 : -719.3513} * {(view_proj[6].x) : 0}))))
<=>
((pf_10_5 * (view_proj[6].w)) + ((pf_9_6 * (view_proj[6].z)) + ((pf_8_6 * (view_proj[6].y)) + (pf_4_17 * (view_proj[6].x)))))
<=>
{pf_11_3 = ((pf_10_5 * 
	{f_2_58 = (view_proj[6].w)
	}) + 
	{pf_12_2 = ((pf_9_6 * 
		{f_2_54 = (view_proj[6].z)
		}) + 
		{pf_13_1 = ((pf_8_6 * 
			{f_2_50 = (view_proj[6].y)
			}) + 
			{pf_13_0 = (pf_4_17 * 
				{f_2_46 = (view_proj[6].x)
				})
			})
		})
	})
}
*/
	f_2_59 = utof(vs_cbuf8[7 ].w  );	// 0
	pf_4_21 = fma(pf_10_5, f_2_59, pf_4_20 );	/* 489.8628  <=>  (({pf_10_5 : 1.00} * {(view_proj[7].w) : 0}) + (({pf_9_6 : -489.8628} * {(view_proj[7].z) : -1}) + (({pf_8_6 : -93.81641} * {(view_proj[7].y) : 0}) + ({pf_4_17 : -719.3513} * {(view_proj[7].x) : 0}))))
<=>
((pf_10_5 * (view_proj[7].w)) + ((pf_9_6 * (view_proj[7].z)) + ((pf_8_6 * (view_proj[7].y)) + (pf_4_17 * (view_proj[7].x)))))
<=>
{pf_4_21 = ((pf_10_5 * 
	{f_2_59 = (view_proj[7].w)
	}) + 
	{pf_4_20 = ((pf_9_6 * 
		{f_2_55 = (view_proj[7].z)
		}) + 
		{pf_4_19 = ((pf_8_6 * 
			{f_2_51 = (view_proj[7].y)
			}) + 
			{pf_4_18 = (pf_4_17 * 
				{f_2_47 = (view_proj[7].x)
				})
			})
		})
	})
}
*/
	pf_10_6 = 0.f * pf_8_8;	// -0
	f_2_60 = utof(vs_cbuf8[29 ].x  );	// -1919.2622
	f_3_11 = 0.f - (pf_7_2 );	// 1134.27
	pf_12_3 = f_3_11 + f_2_60;	/* -784.99207  <=>  ((0.f - {pf_7_2 : -1134.2701}) + {(camera_wpos.x) : -1919.2622})
<=>
((0.f - pf_7_2) + (camera_wpos.x))
<=>
{pf_12_3 = (
	{f_3_11 = (0.f - pf_7_2)
	} + 
	{f_2_60 = (camera_wpos.x)
	})
}
*/
	f_2_61 = utof(vs_cbuf15[52 ].x  );	// -2116
	f_2_62 = 0.f - (f_2_61 );	// 2116.00
	pf_7_3 = pf_7_2 + f_2_62;	// 981.7299
	pf_13_2 = fma(0.f, pf_9_7, pf_10_6 );	/* -0  <=>  ((0.f * (({pf_10_5 : 1.00} * {(view_proj[4].w) : 0}) + (({pf_9_6 : -489.8628} * {(view_proj[4].z) : 0}) + (({pf_8_6 : -93.81641} * {(view_proj[4].y) : 0}) + ({pf_4_17 : -719.3513} * {(view_proj[4].x) : 1.206285}))))) + (0.f * {pf_8_8 : -201.18993}))
<=>
((0.f * ((pf_10_5 * (view_proj[4].w)) + ((pf_9_6 * (view_proj[4].z)) + ((pf_8_6 * (view_proj[4].y)) + (pf_4_17 * (view_proj[4].x)))))) + (0.f * pf_8_8))
<=>
{pf_13_2 = ((0.f * 
	{pf_9_7 = ((pf_10_5 * 
		{f_2_57 = (view_proj[4].w)
		}) + 
		{pf_11_2 = ((pf_9_6 * 
			{f_2_53 = (view_proj[4].z)
			}) + 
			{pf_12_1 = ((pf_8_6 * 
				{f_2_49 = (view_proj[4].y)
				}) + 
				{pf_12_0 = (pf_4_17 * 
					{f_2_44 = (view_proj[4].x)
					})
				})
			})
		})
	}) + 
	{pf_10_6 = (0.f * pf_8_8)
	})
}
*/
	pf_14_0 = pf_12_3 * pf_12_3;	// 616212.60
	f_2_63 = utof(vs_cbuf15[52 ].z  );	// 0.0025
	pf_7_4 = pf_7_3 * f_2_63;	// 2.454324
	pf_15_0 = fma(0.f, pf_11_3, pf_13_2 );	// 0
	pf_13_3 = fma(pf_11_3, 0.5f, pf_13_2 );	// 244.8333
	f_2_64 = utof(vs_cbuf8[29 ].z  );	// -3733.0469
	f_3_12 = 0.f - (pf_1_10 );	// 3936.758
	pf_16_0 = f_3_12 + f_2_64;	/* 203.7114  <=>  ((0.f - {pf_1_10 : -3936.7583}) + {(camera_wpos.z) : -3733.0469})
<=>
((0.f - pf_1_10) + (camera_wpos.z))
<=>
{pf_16_0 = (
	{f_3_12 = (0.f - pf_1_10)
	} + 
	{f_2_64 = (camera_wpos.z)
	})
}
*/
	f_2_65 = utof(vs_cbuf15[52 ].y  );	// -3932
	f_2_66 = 0.f - (f_2_65 );	// 3932.00
	pf_1_11 = pf_1_10 + f_2_66;	// -4.758301
	pf_15_1 = pf_4_21 + pf_15_0;	// 489.8628
	f_2_67 = utof(vs_cbuf8[29 ].y  );	// 365.7373
	f_3_13 = 0.f - (pf_2_6 );	// -36.30958
	pf_17_0 = f_3_13 + f_2_67;	/* 329.4277  <=>  ((0.f - {pf_2_6 : 36.30958}) + {(camera_wpos.y) : 365.7373})
<=>
((0.f - pf_2_6) + (camera_wpos.y))
<=>
{pf_17_0 = (
	{f_3_13 = (0.f - pf_2_6)
	} + 
	{f_2_67 = (camera_wpos.y)
	})
}
*/
	f_2_68 = (1.0f ) / pf_15_1;	// 0.0020414
	pf_13_4 = fma(pf_4_21, 0.5f, pf_13_3 );	// 489.7647
	f_3_14 = utof(vs_cbuf15[52 ].z  );	// 0.0025
	pf_1_12 = pf_1_11 * f_3_14;	// -0.011895752
	pf_14_1 = fma(pf_17_0, pf_17_0, pf_14_0 );	// 724735.20
	pf_14_2 = fma(pf_16_0, pf_16_0, pf_14_1 );	// 766233.60
	pf_18_0 = pf_13_4 * f_2_68;	// 0.9997998
	f_3_15 = inversesqrt(pf_14_2 );	/* 0.0011424  <=>  inversesqrt((({pf_16_0 : 203.7114} * {pf_16_0 : 203.7114}) + (({pf_17_0 : 329.4277} * {pf_17_0 : 329.4277}) + ({pf_12_3 : -784.99207} * {pf_12_3 : -784.99207}))))
<=>
inversesqrt(((pf_16_0 * pf_16_0) + ((pf_17_0 * pf_17_0) + (pf_12_3 * pf_12_3))))
<=>
{f_3_15 = inversesqrt(
	{pf_14_2 = ((pf_16_0 * pf_16_0) + 
		{pf_14_1 = ((pf_17_0 * pf_17_0) + 
			{pf_14_0 = (pf_12_3 * pf_12_3)
			})
		})
	})
}
*/
	f_4_2 = utof(vs_cbuf8[30 ].y  );	// 25000.00
	f_5_1 = utof(vs_cbuf8[30 ].w  );	// 24999.90
	f_4_3 = 0.f - (f_4_2 );	// -25000
	pf_14_3 = fma(pf_18_0, f_5_1, f_4_3 );	// -5.1048894
	f_4_4 = (1.0f ) / pf_14_3;	// -0.19589064
	pf_12_4 = pf_12_3 * f_3_15;	// -0.89677745
	pf_14_4 = pf_17_0 * f_3_15;	// 0.3763393
	pf_16_1 = pf_16_0 * f_3_15;	// 0.2327206
	f_3_16 = utof(vs_cbuf15[28 ].x  );	// 0.8802994
	pf_12_5 = pf_12_4 * f_3_16;	// -0.78943264
	f_3_17 = utof(vs_cbuf8[30 ].z  );	// 2500.00
	f_3_18 = 0.f - (f_3_17 );	// -2500
	pf_17_1 = f_4_4 * f_3_18;	/* 489.7266  <=>  ((1.0f / ((((({pf_4_21 : 489.8628} * 0.5f) + (({pf_11_3 : 489.6667} * 0.5f) + {pf_13_2 : -0})) * (1.0f / ({pf_4_21 : 489.8628} + ((0.f * {pf_11_3 : 489.6667}) + {pf_13_2 : -0})))) * {(vs_cbuf8_30.w) : 24999.90}) + (0.f - {(vs_cbuf8_30.y) : 25000.00}))) * (0.f - {(vs_cbuf8_30.z) : 2500.00}))
<=>
((1.0f / (((((pf_4_21 * 0.5f) + ((pf_11_3 * 0.5f) + pf_13_2)) * (1.0f / (pf_4_21 + ((0.f * pf_11_3) + pf_13_2)))) * (vs_cbuf8_30.w)) + (0.f - (vs_cbuf8_30.y)))) * (0.f - (vs_cbuf8_30.z)))
<=>
{pf_17_1 = (
	{f_4_4 = (1.0f / 
		{pf_14_3 = ((
			{pf_18_0 = (
				{pf_13_4 = ((pf_4_21 * 0.5f) + 
					{pf_13_3 = ((pf_11_3 * 0.5f) + pf_13_2)
					})
				} * 
				{f_2_68 = (1.0f / 
					{pf_15_1 = (pf_4_21 + 
						{pf_15_0 = ((0.f * pf_11_3) + pf_13_2)
						})
					})
				})
			} * 
			{f_5_1 = (vs_cbuf8_30.w)
			}) + 
			{f_4_3 = (0.f - 
				{f_4_2 = (vs_cbuf8_30.y)
				})
			})
		})
	} * 
	{f_3_18 = (0.f - 
		{f_3_17 = (vs_cbuf8_30.z)
		})
	})
}
*/
	f_3_19 = utof(vs_cbuf15[28 ].y  );	// -0.4663191
	pf_12_6 = fma(pf_14_4, f_3_19, pf_12_5 );	// -0.96492684
	f_3_20 = utof(vs_cbuf15[22 ].y  );	// -0.0016638935
	f_4_5 = utof(vs_cbuf15[22 ].x  );	// 0.0000333
	f_3_21 = 0.f - (f_3_20 );	// 0.0016639
	pf_14_5 = fma(pf_17_1, f_4_5, f_3_21 );	// 0.0179718
	f_3_22 = min(max(pf_14_5, 0.0 ), 1.0 );	// 0.0179718
	f_4_6 = utof(vs_cbuf15[28 ].z  );	// -0.08728968
	pf_12_7 = fma(pf_16_1, f_4_6, pf_12_6 );	// -0.98524094
	f_3_23 = 0.f - (f_3_22 );	// -0.017971788
	pf_14_6 = f_3_23 + 1.f;	// 0.9820282
	f_3_24 = log2(pf_14_6 );	// -0.02616366
	pf_12_8 = fma(pf_12_7, 0.5f, 0.5f );	/* 0.0073795  <=>  ((((({pf_16_0 : 203.7114} * {f_3_15 : 0.0011424}) * {(lightDir.z) : -0.08728968}) + ((({pf_17_0 : 329.4277} * {f_3_15 : 0.0011424}) * {(lightDir.y) : -0.4663191}) + (({pf_12_3 : -784.99207} * {f_3_15 : 0.0011424}) * {(lightDir.x) : 0.8802994}))) * 0.5f) + 0.5f)
<=>
(((((pf_16_0 * f_3_15) * (lightDir.z)) + (((pf_17_0 * f_3_15) * (lightDir.y)) + ((pf_12_3 * f_3_15) * (lightDir.x)))) * 0.5f) + 0.5f)
<=>
{pf_12_8 = ((
	{pf_12_7 = ((
		{pf_16_1 = (pf_16_0 * f_3_15)
		} * 
		{f_4_6 = (lightDir.z)
		}) + 
		{pf_12_6 = ((
			{pf_14_4 = (pf_17_0 * f_3_15)
			} * 
			{f_3_19 = (lightDir.y)
			}) + 
			{pf_12_5 = (
				{pf_12_4 = (pf_12_3 * f_3_15)
				} * 
				{f_3_16 = (lightDir.x)
				})
			})
		})
	} * 0.5f) + 0.5f)
}
*/
	pf_14_7 = fma(pf_12_8, -0.0187293f, 0.074260995f );	// 0.0741228
	f_4_7 = 0.f - (pf_12_8 );	// -0.007379532
	pf_16_2 = f_4_7 + 1.f;	// 0.9926205
	f_4_8 = sqrt(pf_16_2 );	// 0.9963034
	f_5_2 = utof(vs_cbuf15[23 ].y  );	// 1.00
	pf_16_3 = f_3_24 * f_5_2;	// -0.02616366
	pf_14_8 = fma(pf_12_8, pf_14_7, -0.2121144f );	// -0.2115674
	pf_12_9 = fma(pf_12_8, pf_14_8, 1.5707288f );	/* 1.569167  <=>  (({pf_12_8 : 0.0073795} * (({pf_12_8 : 0.0073795} * (({pf_12_8 : 0.0073795} * -0.0187293f) + 0.074260995f)) + -0.2121144f)) + 1.5707288f)
<=>
((pf_12_8 * ((pf_12_8 * ((pf_12_8 * -0.0187293f) + 0.074260995f)) + -0.2121144f)) + 1.5707288f)
<=>
{pf_12_9 = ((pf_12_8 * 
	{pf_14_8 = ((pf_12_8 * 
		{pf_14_7 = ((pf_12_8 * -0.0187293f) + 0.074260995f)
		}) + -0.2121144f)
	}) + 1.5707288f)
}
*/
	f_5_3 = exp2(pf_16_3 );	/* 0.9820282  <=>  exp2((log2(((0.f - clamp((({pf_17_1 : 489.7266} * {(vs_cbuf15_22.x) : 0.0000333}) + (0.f - {(vs_cbuf15_22.y) : -0.0016638935})), 0.0, 1.0)) + 1.f)) * {(vs_cbuf15_23.y) : 1.00}))
<=>
exp2((log2(((0.f - clamp(((pf_17_1 * (vs_cbuf15_22.x)) + (0.f - (vs_cbuf15_22.y))), 0.0, 1.0)) + 1.f)) * (vs_cbuf15_23.y)))
<=>
{f_5_3 = exp2(
	{pf_16_3 = (
		{f_3_24 = log2(
			{pf_14_6 = (
				{f_3_23 = (0.f - 
					{f_3_22 = clamp(
						{pf_14_5 = ((pf_17_1 * 
							{f_4_5 = (vs_cbuf15_22.x)
							}) + 
							{f_3_21 = (0.f - 
								{f_3_20 = (vs_cbuf15_22.y)
								})
							})
						}, 0.0, 1.0)
					})
				} + 1.f)
			})
		} * 
		{f_5_2 = (vs_cbuf15_23.y)
		})
	})
}
*/
	f_4_9 = 0.f - (f_4_8 );	// -0.9963034
	pf_12_10 = pf_12_9 * f_4_9;	// -1.5633669
	pf_12_11 = fma(pf_12_10, 0.63661975f, 1.f );	// 0.0047298
	pf_14_9 = fma(f_5_3, 0.5f, 0.5f );	// 0.9910141
	f2_0_0 = vec2(pf_12_11, pf_14_9 );	// vec2(0.0047298,0.9910141)
	f4_0_0 = textureLod(tex0, f2_0_0, 0.0 );	/* vec4(0.50,0.50,0.50,1.00)  <=>  textureLod({tex0 : tex0}, vec2(((({pf_12_9 : 1.569167} * (0.f - sqrt(((0.f - {pf_12_8 : 0.0073795}) + 1.f)))) * 0.63661975f) + 1.f), (({f_5_3 : 0.9820282} * 0.5f) + 0.5f)), 0.0, s_linear_clamp_sampler)
<=>
textureLod(tex0, vec2((((pf_12_9 * (0.f - sqrt(((0.f - pf_12_8) + 1.f)))) * 0.63661975f) + 1.f), ((f_5_3 * 0.5f) + 0.5f)), 0.0, s_linear_clamp_sampler)
<=>
{f4_0_0 = textureLod(tex0, 
{f2_0_0 = vec2(
	{pf_12_11 = ((
		{pf_12_10 = (pf_12_9 * 
			{f_4_9 = (0.f - 
				{f_4_8 = sqrt(
					{pf_16_2 = (
						{f_4_7 = (0.f - pf_12_8)
						} + 1.f)
					})
				})
			})
		} * 0.63661975f) + 1.f)
	}, 
	{pf_14_9 = ((f_5_3 * 0.5f) + 0.5f)
	})
}, 0.0, s_linear_clamp_sampler)
}
*/
	f_4_10 = f4_0_0.x ;	// 0.50
	f_5_4 = f4_0_0.y ;	// 0.50
	f_6_1 = f4_0_0.z ;	// 0.50
	f2_0_1 = vec2(pf_7_4, pf_1_12 );	// vec2(2.454324,-0.011895752)
	f4_0_1 = textureLod(tex1, f2_0_1, 0.0 );	/* vec4(0.50,0.50,0.50,1.00)  <=>  textureLod({tex1 : tex1}, vec2((({pf_7_2 : -1134.2701} + (0.f - {(vs_cbuf15_52.x) : -2116})) * {(vs_cbuf15_52.z) : 0.0025}), (({pf_1_10 : -3936.7583} + (0.f - {(vs_cbuf15_52.y) : -3932})) * {(vs_cbuf15_52.z) : 0.0025})), 0.0, s_linear_clamp_sampler)
<=>
textureLod(tex1, vec2(((pf_7_2 + (0.f - (vs_cbuf15_52.x))) * (vs_cbuf15_52.z)), ((pf_1_10 + (0.f - (vs_cbuf15_52.y))) * (vs_cbuf15_52.z))), 0.0, s_linear_clamp_sampler)
<=>
{f4_0_1 = textureLod(tex1, 
{f2_0_1 = vec2(
	{pf_7_4 = (
		{pf_7_3 = (pf_7_2 + 
			{f_2_62 = (0.f - 
				{f_2_61 = (vs_cbuf15_52.x)
				})
			})
		} * 
		{f_2_63 = (vs_cbuf15_52.z)
		})
	}, 
	{pf_1_12 = (
		{pf_1_11 = (pf_1_10 + 
			{f_2_66 = (0.f - 
				{f_2_65 = (vs_cbuf15_52.y)
				})
			})
		} * 
		{f_3_14 = (vs_cbuf15_52.z)
		})
	})
}, 0.0, s_linear_clamp_sampler)
}
*/
	f_7_24 = f4_0_1.w ;	// 1.00
	u_0_5 = (vs_cbuf0[21 ].x  );	// 675610624
	f_8_1 = in_attr7.w ;	// 0.42892
	f_9_2 = float(0u );	// 0
	u_1_4 = u_0_5 + 128u;	// 675610752
	u_2_7 = (vs_cbuf0[21 ].x  );	// 675610624
	u_1_5 = u_1_4 - u_2_7;	/* 128  <=>  ((ftou({vs_cbuf0_21.x : 1.0935697E-14}) + 128u) - ftou({vs_cbuf0_21.x : 1.0935697E-14}))
<=>
((ftou(vs_cbuf0_21.x) + 128u) - ftou(vs_cbuf0_21.x))
<=>
{u_1_5 = (
	{u_1_4 = (
		{u_0_5 = ftou(vs_cbuf0_21.x)
		} + 128u)
	} - 
	{u_2_7 = ftou(vs_cbuf0_21.x)
	})
}
*/
	u4_0_0 = uvec4(vs_ssbo0[u_1_5 >> 2 ], vs_ssbo0[(u_1_5 + 4 ) >> 2 ], vs_ssbo0[(u_1_5 + 8 ) >> 2 ], vs_ssbo0[(u_1_5 + 12 ) >> 2 ] );
	u_1_6 = u4_0_0.x ;
	u_2_8 = u4_0_0.y ;
	u_3_8 = u4_0_0.z ;
	u_0_6 = u_0_5 + 112u;	// 675610736
	u_4_7 = (vs_cbuf0[21 ].x  );	// 675610624
	u_0_7 = u_0_6 - u_4_7;	/* 112  <=>  ((ftou({vs_cbuf0_21.x : 1.0935697E-14}) + 112u) - ftou({vs_cbuf0_21.x : 1.0935697E-14}))
<=>
((ftou(vs_cbuf0_21.x) + 112u) - ftou(vs_cbuf0_21.x))
<=>
{u_0_7 = (
	{u_0_6 = (
		{u_0_5 = ftou(vs_cbuf0_21.x)
		} + 112u)
	} - 
	{u_4_7 = ftou(vs_cbuf0_21.x)
	})
}
*/
	u4_0_1 = uvec4(vs_ssbo0[u_0_7 >> 2 ], vs_ssbo0[(u_0_7 + 4 ) >> 2 ], vs_ssbo0[(u_0_7 + 8 ) >> 2 ], vs_ssbo0[(u_0_7 + 12 ) >> 2 ] );
	u_0_8 = u4_0_1.x ;	/* {uvec4({vs_ssbo_color1.x * vs_ssbo_scale : }, {vs_ssbo_color1.y * vs_ssbo_scale : }, {vs_ssbo_color1.z * vs_ssbo_scale : }, {vs_ssbo_color1.w * vs_ssbo_scale : }).x : }
<=>
uvec4(vs_ssbo_color1.x * vs_ssbo_scale, vs_ssbo_color1.y * vs_ssbo_scale, vs_ssbo_color1.z * vs_ssbo_scale, vs_ssbo_color1.w * vs_ssbo_scale).x
<=>
{u_0_8 = 
	{u4_0_1 = uvec4(vs_ssbo_color1.x * vs_ssbo_scale, vs_ssbo_color1.y * vs_ssbo_scale, vs_ssbo_color1.z * vs_ssbo_scale, vs_ssbo_color1.w * vs_ssbo_scale)
	}.x
}
*/
	u_4_8 = u4_0_1.y ;	/* {uvec4({vs_ssbo_color1.x * vs_ssbo_scale : }, {vs_ssbo_color1.y * vs_ssbo_scale : }, {vs_ssbo_color1.z * vs_ssbo_scale : }, {vs_ssbo_color1.w * vs_ssbo_scale : }).y : }
<=>
uvec4(vs_ssbo_color1.x * vs_ssbo_scale, vs_ssbo_color1.y * vs_ssbo_scale, vs_ssbo_color1.z * vs_ssbo_scale, vs_ssbo_color1.w * vs_ssbo_scale).y
<=>
{u_4_8 = 
	{u4_0_1 = uvec4(vs_ssbo_color1.x * vs_ssbo_scale, vs_ssbo_color1.y * vs_ssbo_scale, vs_ssbo_color1.z * vs_ssbo_scale, vs_ssbo_color1.w * vs_ssbo_scale)
	}.y
}
*/
	u_5_8 = u4_0_1.z ;	/* {uvec4({vs_ssbo_color1.x * vs_ssbo_scale : }, {vs_ssbo_color1.y * vs_ssbo_scale : }, {vs_ssbo_color1.z * vs_ssbo_scale : }, {vs_ssbo_color1.w * vs_ssbo_scale : }).z : }
<=>
uvec4(vs_ssbo_color1.x * vs_ssbo_scale, vs_ssbo_color1.y * vs_ssbo_scale, vs_ssbo_color1.z * vs_ssbo_scale, vs_ssbo_color1.w * vs_ssbo_scale).z
<=>
{u_5_8 = 
	{u4_0_1 = uvec4(vs_ssbo_color1.x * vs_ssbo_scale, vs_ssbo_color1.y * vs_ssbo_scale, vs_ssbo_color1.z * vs_ssbo_scale, vs_ssbo_color1.w * vs_ssbo_scale)
	}.z
}
*/
	f_11_2 = utof(vs_cbuf9[78 ].z  );	// 1.00
	f_13_9 = trunc(f_11_2 );	// 1.00
	f_13_10 = min(max(f_13_9, float(-2147483600.f ) ), float(2147483600.f ) );	// 1.00
	u_6_6 = int(f_13_10 );	// 1
	b_0_6 = isnan(f_11_2 );	// False
	u_6_7 = b_0_6 ? (0u) : (u_6_6);	/* 1  <=>  (isnan({utof(vs_cbuf9[78].z) : 1.00}) ? 0u : int(clamp(trunc({utof(vs_cbuf9[78].z) : 1.00}), float(-2147483600.f), float(2147483600.f))))
<=>
(isnan(utof(vs_cbuf9[78].z)) ? 0u : int(clamp(trunc(utof(vs_cbuf9[78].z)), float(-2147483600.f), float(2147483600.f))))
<=>
{u_6_7 = (
	{b_0_6 = isnan(
		{f_11_2 = utof(vs_cbuf9[78].z)
		})
	} ? 0u : 
	{u_6_6 = int(
		{f_13_10 = clamp(
			{f_13_9 = trunc(f_11_2)
			}, float(-2147483600.f), float(2147483600.f))
		})
	})
}
*/
	f_11_3 = in_attr7.y ;	// 0.96051
	f_13_11 = in_attr7.z ;	// 0.58835
	u_7_1 = uint(bitfieldExtract(int(u_6_7 ), int(0u ), int(32u ) ) );	// 1
	u_7_2 = abs(int(u_7_1 ) );	// 1
	u_7_3 = uint(bitfieldExtract(uint(u_7_2 ), int(0u ), int(32u ) ) );	/* 1  <=>  uint(bitfieldExtract(uint(abs(int(uint(bitfieldExtract(int({u_6_7 : 1}), int(0u), int(32u)))))), int(0u), int(32u)))
<=>
uint(bitfieldExtract(uint(abs(int(uint(bitfieldExtract(int(u_6_7), int(0u), int(32u)))))), int(0u), int(32u)))
<=>
{u_7_3 = uint(bitfieldExtract(uint(
	{u_7_2 = abs(int(
		{u_7_1 = uint(bitfieldExtract(int(u_6_7), int(0u), int(32u)))
		}))
	}), int(0u), int(32u)))
}
*/
	u_8_1 = (vs_cbuf9[7 ].y  );	// 0
	u_8_2 = 1u & u_8_1;	// 0
	f_14_2 = float(u_6_7 );	// 1.00
	u_9_1 = (vs_cbuf9[7 ].y  );	// 0
	u_9_2 = 4u & u_9_1;	// 0
	u_10_0 = uint(bitfieldExtract(uint(u_6_7 ), int(0u ), int(32u ) ) );	// 1
	u_10_1 = uint(bitfieldExtract(uint(u_10_0 ), int(0u ), int(32u ) ) );	// 1
	b_0_7 = u_10_1 == 0;	// False
	b_1_22 = int(u_10_1 ) < 0;	/* False  <=>  (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(uint({u_6_7 : 1}), int(0u), int(32u)))), int(0u), int(32u)))) < 0)
<=>
(int(uint(bitfieldExtract(uint(uint(bitfieldExtract(uint(u_6_7), int(0u), int(32u)))), int(0u), int(32u)))) < 0)
<=>
{b_1_22 = (int(
	{u_10_1 = uint(bitfieldExtract(uint(
		{u_10_0 = uint(bitfieldExtract(uint(u_6_7), int(0u), int(32u)))
		}), int(0u), int(32u)))
	}) < 0)
}
*/
	out_attr7.x  = 1.f;	/* 1.00  <=>  1.f
<=>
1.f
<=>
{out_attr7.x = 1.f
}
*/
	b_2_1 = u_8_2 == 1u;	// False
	u_8_3 = b_2_1 ? (4294967295u) : (0u);	// 0
	f_15_0 = in_attr3.x ;	// 1.00
	f_16_0 = utof(vs_cbuf15[28 ].y  );	// -0.4663191
	pf_1_13 = fma(f_16_0, 1.5f, 1.5f );	// 0.8005213
	f_16_1 = min(max(pf_1_13, 0.0 ), 1.0 );	// 0.8005213
	f_17_0 = float(u_7_3 );	// 1.00
	f_18_0 = utof(vs_cbuf9[75 ].y  );	// 1.00
	pf_1_14 = f_11_3 * f_18_0;	// 0.96051
	f_14_3 = (1.0f ) / f_14_2;	// 1.00
	u_11_0 = ftou(f_14_3 );	// 1065353216
	u_11_1 = u_11_0 + 4294967294u;	// 1065353214
	f_14_4 = (1.0f ) / f_17_0;	/* 1.00  <=>  (1.0f / float({u_7_3 : 1}))
<=>
(1.0f / float(u_7_3))
<=>
{f_14_4 = (1.0f / 
	{f_17_0 = float(u_7_3)
	})
}
*/
	u_12_0 = ftou(f_14_4 );	// 1065353216
	f_14_5 = utof(vs_cbuf9[77 ].y  );	// 0
	f_17_1 = utof(vs_cbuf9[77 ].z  );	// 0
	pf_7_5 = f_17_1 + f_14_5;	// 0
	f_14_6 = in_attr1.y ;	// 0.50196
	u_13_0 = ftou(f_14_6 );	// 1056997491
	u_12_1 = u_12_0 + 4294967294u;	/* 1065353214  <=>  ({ftou(f_14_4) : 1065353216} + 4294967294u)
<=>
(ftou(f_14_4) + 4294967294u)
<=>
{u_12_1 = (
	{u_12_0 = ftou(f_14_4)
	} + 4294967294u)
}
*/
	out_attr5.x  = f_15_0;	/* 1.00  <=>  {in_attr3.x : 1.00}
<=>
in_attr3.x
<=>
{out_attr5.x = 
	{f_15_0 = in_attr3.x
	}
}
*/
	u_14_0 = (vs_cbuf9[7 ].y  );	// 0
	u_14_1 = 8u & u_14_0;	// 0
	f_15_1 = (1.0f ) / f_16_1;	// 1.249186
	b_2_2 = f_8_1 > 0.5f && ! isnan(f_8_1 ) && ! isnan(0.5f );	// False
	u_15_0 = b_2_2 ? (4294967295u) : (0u);	// 0
	gl_Position.y  = pf_8_8;	/* -201.18993  <=>  {pf_8_8 : -201.18993}
<=>
pf_8_8
<=>
{gl_Position.y = pf_8_8
}
*/
	f_8_2 = utof(vs_cbuf9[74 ].w  );	// 0
	f_16_2 = utof(vs_cbuf9[75 ].y  );	// 1.00
	pf_12_12 = f_16_2 + f_8_2;	// 1.00
	out_attr3.z  = pf_13_4;	/* 489.7647  <=>  (({pf_4_21 : 489.8628} * 0.5f) + (({pf_11_3 : 489.6667} * 0.5f) + {pf_13_2 : -0}))
<=>
((pf_4_21 * 0.5f) + ((pf_11_3 * 0.5f) + pf_13_2))
<=>
{out_attr3.z = 
	{pf_13_4 = ((pf_4_21 * 0.5f) + 
		{pf_13_3 = ((pf_11_3 * 0.5f) + pf_13_2)
		})
	}
}
*/
	f_8_3 = utof(u_11_1 );	// 0.9999999
	pf_13_5 = f_9_2 * f_8_3;	// 0
	gl_Position.x  = pf_9_7;	/* -867.7427  <=>  (({pf_10_5 : 1.00} * {(view_proj[4].w) : 0}) + (({pf_9_6 : -489.8628} * {(view_proj[4].z) : 0}) + (({pf_8_6 : -93.81641} * {(view_proj[4].y) : 0}) + ({pf_4_17 : -719.3513} * {(view_proj[4].x) : 1.206285}))))
<=>
((pf_10_5 * (view_proj[4].w)) + ((pf_9_6 * (view_proj[4].z)) + ((pf_8_6 * (view_proj[4].y)) + (pf_4_17 * (view_proj[4].x)))))
<=>
{gl_Position.x = 
	{pf_9_7 = ((pf_10_5 * 
		{f_2_57 = (view_proj[4].w)
		}) + 
		{pf_11_2 = ((pf_9_6 * 
			{f_2_53 = (view_proj[4].z)
			}) + 
			{pf_12_1 = ((pf_8_6 * 
				{f_2_49 = (view_proj[4].y)
				}) + 
				{pf_12_0 = (pf_4_17 * 
					{f_2_44 = (view_proj[4].x)
					})
				})
			})
		})
	}
}
*/
	f_8_4 = utof(u_12_1 );	// 0.9999999
	pf_14_10 = f_8_4 * f_9_2;	// 0
	f_8_5 = trunc(pf_13_5 );	// 0
	f_8_6 = min(max(f_8_5, float(0.f ) ), float(4294967300.f ) );	/* 0  <=>  clamp(trunc((float(0u) * {utof(({ftou((1.0f / float({u_6_7 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999})), float(0.f), float(4294967300.f))
<=>
clamp(trunc((float(0u) * utof((ftou((1.0f / float(u_6_7))) + 4294967294u)))), float(0.f), float(4294967300.f))
<=>
{f_8_6 = clamp(
	{f_8_5 = trunc(
		{pf_13_5 = (
			{f_9_2 = float(0u)
			} * 
			{f_8_3 = utof(
				{u_11_1 = (
					{u_11_0 = ftou(
						{f_14_3 = (1.0f / 
							{f_14_2 = float(u_6_7)
							})
						})
					} + 4294967294u)
				})
			})
		})
	}, float(0.f), float(4294967300.f))
}
*/
	u_16_0 = uint(f_8_6 );	/* 0  <=>  uint({f_8_6 : 0})
<=>
uint(f_8_6)
<=>
{u_16_0 = uint(f_8_6)
}
*/
	b_2_3 = u_9_2 == 4u;	// False
	u_9_3 = b_2_3 ? (4294967295u) : (0u);	// 0
	f_8_7 = trunc(pf_14_10 );	// 0
	f_8_8 = min(max(f_8_7, float(0.f ) ), float(4294967300.f ) );	// 0
	u_17_0 = uint(f_8_8 );	/* 0  <=>  uint(clamp(trunc(({utof(u_12_1) : 0.9999999} * float(0u))), float(0.f), float(4294967300.f)))
<=>
uint(clamp(trunc((utof(u_12_1) * float(0u))), float(0.f), float(4294967300.f)))
<=>
{u_17_0 = uint(
	{f_8_8 = clamp(
		{f_8_7 = trunc(
			{pf_14_10 = (
				{f_8_4 = utof(u_12_1)
				} * 
				{f_9_2 = float(0u)
				})
			})
		}, float(0.f), float(4294967300.f))
	})
}
*/
	pf_1_15 = fma(pf_1_14, -2.f, pf_12_12 );	// -0.92102003
	gl_Position.z  = pf_11_3;	/* 489.6667  <=>  {pf_11_3 : 489.6667}
<=>
pf_11_3
<=>
{gl_Position.z = pf_11_3
}
*/
	b_2_4 = f_12_3 == 0.f && ! isnan(f_12_3 ) && ! isnan(0.f );	// True
	gl_Position.w  = pf_4_21;	/* 489.8628  <=>  {pf_4_21 : 489.8628}
<=>
pf_4_21
<=>
{gl_Position.w = pf_4_21
}
*/
	b_3_0 = u_14_1 == 8u;	// False
	u_14_2 = b_3_0 ? (4294967295u) : (0u);	// 0
	f_8_9 = in_attr1.x ;	// 0.50196
	u_18_0 = ftou(f_8_9 );	// 1056997491
	b_3_1 = f_13_11 > 0.5f && ! isnan(f_13_11 ) && ! isnan(0.5f );	// True
	u_19_0 = b_3_1 ? (4294967295u) : (0u);	// 4294967295
	out_attr3.w  = pf_15_1;	/* 489.8628  <=>  ({pf_4_21 : 489.8628} + ((0.f * {pf_11_3 : 489.6667}) + {pf_13_2 : -0}))
<=>
(pf_4_21 + ((0.f * pf_11_3) + pf_13_2))
<=>
{out_attr3.w = 
	{pf_15_1 = (pf_4_21 + 
		{pf_15_0 = ((0.f * pf_11_3) + pf_13_2)
		})
	}
}
*/
	b_3_2 = f_0_6 > 0.5f && ! isnan(f_0_6 ) && ! isnan(0.5f );	// True
	u_20_0 = b_3_2 ? (4294967295u) : (0u);	// 4294967295
	out_attr8.w  = 0.f;	/* 0  <=>  0.f
<=>
0.f
<=>
{out_attr8.w = 0.f
}
*/
	f_9_3 = utof(vs_cbuf9[77 ].z  );	// 0
	pf_7_6 = fma(f_13_11, f_9_3, pf_7_5 );	// 0
	u_21_0 = uint(bitfieldExtract(uint(u_6_7 ), int(0u ), int(16u ) ) );	// 1
	u_22_0 = uint(bitfieldExtract(uint(u_16_0 ), int(0u ), int(16u ) ) );	// 0
	u_21_1 = uint(u_21_0 * u_22_0 );	// 0
	u_22_1 = uint(bitfieldExtract(uint(u_6_7 ), int(0u ), int(16u ) ) );	// 1
	u_23_0 = uint(bitfieldExtract(uint(u_16_0 ), int(16u ), int(16u ) ) );	// 0
	u_22_2 = uint(u_22_1 * u_23_0 );	// 0
	u_23_1 = uint(bitfieldExtract(uint(u_16_0 ), int(0u ), int(16u ) ) );	// 0
	u_22_3 = bitfieldInsert(u_22_2, u_23_1, int(16u ), int(16u ) );	// 0
	u_23_2 = uint(bitfieldExtract(uint(u_17_0 ), int(0u ), int(16u ) ) );	// 0
	u_24_0 = uint(bitfieldExtract(uint(u_7_3 ), int(0u ), int(16u ) ) );	// 1
	u_23_3 = uint(u_23_2 * u_24_0 );	// 0
	u_24_1 = uint(bitfieldExtract(uint(u_17_0 ), int(0u ), int(16u ) ) );	// 0
	u_25_0 = uint(bitfieldExtract(uint(u_7_3 ), int(16u ), int(16u ) ) );	// 0
	u_24_2 = uint(u_24_1 * u_25_0 );	// 0
	u_25_1 = uint(bitfieldExtract(uint(u_7_3 ), int(0u ), int(16u ) ) );	// 1
	u_24_3 = bitfieldInsert(u_24_2, u_25_1, int(16u ), int(16u ) );	// 65536
	u_14_3 = ~ u_14_2;	// 4294967295
	u_15_1 = ~ u_15_0;	// 4294967295
	u_14_4 = u_14_3 | u_15_1;	// 4294967295
	b_3_3 = u_14_4 != 0u;	// True
	f_9_4 = in_attr2.y ;	// 0
	u_14_5 = ftou(f_9_4 );	// 0
	u_9_4 = ~ u_9_3;	// 4294967295
	u_15_2 = ~ u_19_0;	// 0
	u_9_5 = u_9_4 | u_15_2;	// 4294967295
	b_4_0 = u_9_5 != 0u;	// True
	u_8_4 = ~ u_8_3;	// 4294967295
	u_9_6 = ~ u_20_0;	// 0
	u_8_5 = u_8_4 | u_9_6;	// 4294967295
	b_5_0 = u_8_5 != 0u;	// True
	u_8_6 = uint(bitfieldExtract(uint(u_6_7 ), int(16u ), int(16u ) ) );	// 0
	u_9_7 = uint(bitfieldExtract(uint(u_22_3 ), int(16u ), int(16u ) ) );	/* 0  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_6_7 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_16_0 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_16_0 : 0}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
<=>
uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_6_7), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_16_0), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_16_0), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
<=>
{u_9_7 = uint(bitfieldExtract(uint(
	{u_22_3 = bitfieldInsert(
		{u_22_2 = uint((
			{u_22_1 = uint(bitfieldExtract(uint(u_6_7), int(0u), int(16u)))
			} * 
			{u_23_0 = uint(bitfieldExtract(uint(u_16_0), int(16u), int(16u)))
			}))
		}, 
		{u_23_1 = uint(bitfieldExtract(uint(u_16_0), int(0u), int(16u)))
		}, int(16u), int(16u))
	}), int(16u), int(16u)))
}
*/
	u_8_7 = uint(u_8_6 * u_9_7 );	// 0
	u_8_8 = u_8_7 << 16u;	// 0
	u_9_8 = u_22_3 << 16u;	// 0
	u_9_9 = u_9_8 + u_21_1;	/* 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_6_7 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_16_0 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_16_0 : 0}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_6_7 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_16_0 : 0}), int(0u), int(16u))))))
<=>
((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_6_7), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_16_0), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_16_0), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_6_7), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_16_0), int(0u), int(16u))))))
<=>
{u_9_9 = (
	{u_9_8 = (
		{u_22_3 = bitfieldInsert(
			{u_22_2 = uint((
				{u_22_1 = uint(bitfieldExtract(uint(u_6_7), int(0u), int(16u)))
				} * 
				{u_23_0 = uint(bitfieldExtract(uint(u_16_0), int(16u), int(16u)))
				}))
			}, 
			{u_23_1 = uint(bitfieldExtract(uint(u_16_0), int(0u), int(16u)))
			}, int(16u), int(16u))
		} << 16u)
	} + 
	{u_21_1 = uint((
		{u_21_0 = uint(bitfieldExtract(uint(u_6_7), int(0u), int(16u)))
		} * 
		{u_22_0 = uint(bitfieldExtract(uint(u_16_0), int(0u), int(16u)))
		}))
	})
}
*/
	u_8_9 = u_8_8 + u_9_9;	// 0
	u_9_10 = uint(bitfieldExtract(uint(u_17_0 ), int(16u ), int(16u ) ) );	// 0
	u_15_3 = uint(bitfieldExtract(uint(u_24_3 ), int(16u ), int(16u ) ) );	/* 1  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_17_0 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_7_3 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_7_3 : 1}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
<=>
uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_17_0), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_7_3), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_7_3), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
<=>
{u_15_3 = uint(bitfieldExtract(uint(
	{u_24_3 = bitfieldInsert(
		{u_24_2 = uint((
			{u_24_1 = uint(bitfieldExtract(uint(u_17_0), int(0u), int(16u)))
			} * 
			{u_25_0 = uint(bitfieldExtract(uint(u_7_3), int(16u), int(16u)))
			}))
		}, 
		{u_25_1 = uint(bitfieldExtract(uint(u_7_3), int(0u), int(16u)))
		}, int(16u), int(16u))
	}), int(16u), int(16u)))
}
*/
	u_9_11 = uint(u_9_10 * u_15_3 );	// 0
	u_9_12 = u_9_11 << 16u;	// 0
	u_15_4 = u_24_3 << 16u;	// 0
	u_15_5 = u_15_4 + u_23_3;	/* 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_17_0 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_7_3 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_7_3 : 1}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_17_0 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_7_3 : 1}), int(0u), int(16u))))))
<=>
((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_17_0), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_7_3), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_7_3), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_17_0), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_7_3), int(0u), int(16u))))))
<=>
{u_15_5 = (
	{u_15_4 = (
		{u_24_3 = bitfieldInsert(
			{u_24_2 = uint((
				{u_24_1 = uint(bitfieldExtract(uint(u_17_0), int(0u), int(16u)))
				} * 
				{u_25_0 = uint(bitfieldExtract(uint(u_7_3), int(16u), int(16u)))
				}))
			}, 
			{u_25_1 = uint(bitfieldExtract(uint(u_7_3), int(0u), int(16u)))
			}, int(16u), int(16u))
		} << 16u)
	} + 
	{u_23_3 = uint((
		{u_23_2 = uint(bitfieldExtract(uint(u_17_0), int(0u), int(16u)))
		} * 
		{u_24_0 = uint(bitfieldExtract(uint(u_7_3), int(0u), int(16u)))
		}))
	})
}
*/
	u_9_13 = u_9_12 + u_15_5;	// 0
	f_13_12 = in_attr2.x ;	// 0
	u_15_6 = ftou(f_13_12 );	// 0
	b_6_0 = f_10_2 == 0.f && ! isnan(f_10_2 ) && ! isnan(0.f );	// True
	b_2_5 = b_6_0 && b_2_4;	// True
	f_16_3 = utof(vs_cbuf9[76 ].w  );	// 0
	f_17_2 = utof(vs_cbuf9[76 ].y  );	// 2.00
	pf_12_13 = f_17_2 + f_16_3;	// 2.00
	u_8_10 = uint(int(0 ) - int(u_8_9 ) );	/* 0  <=>  uint((int(0) - int(((uint((uint(bitfieldExtract(uint({u_6_7 : 1}), int(16u), int(16u))) * {u_9_7 : 0})) << 16u) + {u_9_9 : 0}))))
<=>
uint((int(0) - int(((uint((uint(bitfieldExtract(uint(u_6_7), int(16u), int(16u))) * u_9_7)) << 16u) + u_9_9))))
<=>
{u_8_10 = uint((int(0) - int(
	{u_8_9 = (
		{u_8_8 = (
			{u_8_7 = uint((
				{u_8_6 = uint(bitfieldExtract(uint(u_6_7), int(16u), int(16u)))
				} * u_9_7))
			} << 16u)
		} + u_9_9)
	})))
}
*/
	u_9_14 = uint(int(0 ) - int(u_9_13 ) );	/* 0  <=>  uint((int(0) - int(((uint((uint(bitfieldExtract(uint({u_17_0 : 0}), int(16u), int(16u))) * {u_15_3 : 1})) << 16u) + {u_15_5 : 0}))))
<=>
uint((int(0) - int(((uint((uint(bitfieldExtract(uint(u_17_0), int(16u), int(16u))) * u_15_3)) << 16u) + u_15_5))))
<=>
{u_9_14 = uint((int(0) - int(
	{u_9_13 = (
		{u_9_12 = (
			{u_9_11 = uint((
				{u_9_10 = uint(bitfieldExtract(uint(u_17_0), int(16u), int(16u)))
				} * u_15_3))
			} << 16u)
		} + u_15_5)
	})))
}
*/
	f_16_4 = float(u_8_10 );	// 0
	f_17_3 = float(u_9_14 );	// 0
	f_18_1 = utof(vs_cbuf9[83 ].w  );	// 1.00
	f_18_2 = (1.0f ) / f_18_1;	// 1.00
	f_19_0 = utof(vs_cbuf9[76 ].w  );	// 0
	pf_12_14 = fma(f_11_3, f_19_0, pf_12_13 );	// 2.00
	b_6_1 = f_11_3 > 0.5f && ! isnan(f_11_3 ) && ! isnan(0.5f );	// True
	u_8_11 = b_6_1 ? (4294967295u) : (0u);	// 4294967295
	u_9_15 = (vs_cbuf9[7 ].y  );	// 0
	u_9_16 = 2u & u_9_15;	// 0
	f_11_4 = utof(vs_cbuf9[74 ].z  );	// 0
	f_19_1 = utof(vs_cbuf9[75 ].x  );	// 1.00
	pf_13_6 = f_19_1 + f_11_4;	// 1.00
	f_11_5 = in_attr2.z ;	// 1.00
	u_19_1 = ftou(f_11_5 );	// 1065353216
	b_6_2 = ! b_4_0;	// False
	b_7_0 = b_6_2 ? (true) : (false);	// False
	if(b_7_0 )	/* False  <=>  if(((! (((~ (((4u & {vs_cbuf9_7_y : 0}) == 4u) ? 4294967295u : 0u)) | (~ (((({in_attr7.z : 0.58835} > 0.5f) && (! isnan({in_attr7.z : 0.58835}))) && (! isnan(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
<=>
if(((! (((~ (((4u & vs_cbuf9_7_y) == 4u) ? 4294967295u : 0u)) | (~ ((((in_attr7.z > 0.5f) && (! isnan(in_attr7.z))) && (! isnan(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
<=>if(b_7_0...)
*/
	{
	} 
	b_3_4 = ! b_3_3;	// False
	b_6_3 = b_3_4 ? (true) : (false);	// False
	u_20_1 = u_13_0;	/* 1056997491  <=>  {ftou(in_attr1.y) : 1056997491}
<=>
ftou(in_attr1.y)
<=>
{u_20_1 = 
	{u_13_0 = ftou(
		{f_14_6 = in_attr1.y
		})
	}
}
*/
	f_19_phi_23 = f_19_1;
	pf_14_phi_23 = pf_14_10;
	u_21_phi_23 = u_21_1;
	u_20_phi_23 = u_20_1;
	if(b_6_3 )	/* False  <=>  if(((! (((~ (((8u & {vs_cbuf9_7_y : 0}) == 8u) ? 4294967295u : 0u)) | (~ (((({in_attr7.w : 0.42892} > 0.5f) && (! isnan({in_attr7.w : 0.42892}))) && (! isnan(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
<=>
if(((! (((~ (((8u & vs_cbuf9_7_y) == 8u) ? 4294967295u : 0u)) | (~ ((((in_attr7.w > 0.5f) && (! isnan(in_attr7.w))) && (! isnan(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
<=>if(b_6_3...)
*/
	{
		f_19_2 = 0.f - (f_14_6 );	// -0.50196
		pf_14_11 = f_19_2 + 1.f;	// 0.49804
		u_21_2 = ftou(pf_14_11 );	// 1056898842
		u_20_2 = u_21_2;	/* 1056898842  <=>  {ftou(((0.f - {in_attr1.y : 0.50196}) + 1.f)) : 1056898842}
<=>
ftou(((0.f - in_attr1.y) + 1.f))
<=>
{u_20_2 = 
	{u_21_2 = ftou(
		{pf_14_11 = (
			{f_19_2 = (0.f - 
				{f_14_6 = in_attr1.y
				})
			} + 1.f)
		})
	}
}
*/
		f_19_phi_23 = f_19_2;
		pf_14_phi_23 = pf_14_11;
		u_21_phi_23 = u_21_2;
		u_20_phi_23 = u_20_2;
	} 
	b_3_5 = f_1_6 == 0.f && ! isnan(f_1_6 ) && ! isnan(0.f );	// False
	b_2_6 = b_3_5 && b_2_5;	// False
	b_3_6 = u_9_16 == 2u;	// False
	u_9_17 = b_3_6 ? (4294967295u) : (0u);	// 0
	f_19_3 = utof(vs_cbuf9[83 ].y  );	// 1.00
	f_20_0 = utof(u_20_phi_23 );	// 0.50196
	pf_14_12 = f_20_0 * f_19_3;	// 0.50196
	f_19_4 = utof(vs_cbuf9[83 ].z  );	// 1.00
	f_19_5 = (1.0f ) / f_19_4;	// 1.00
	b_3_7 = ! b_4_0;	// False
	b_4_1 = b_3_7 ? (true) : (false);	// False
	u_20_3 = u_18_0;	/* 1056997491  <=>  {ftou(in_attr1.x) : 1056997491}
<=>
ftou(in_attr1.x)
<=>
{u_20_3 = 
	{u_18_0 = ftou(
		{f_8_9 = in_attr1.x
		})
	}
}
*/
	f_20_phi_24 = f_20_0;
	pf_15_phi_24 = pf_15_1;
	u_21_phi_24 = u_21_phi_23;
	u_20_phi_24 = u_20_3;
	if(b_4_1 )	/* False  <=>  if(((! (((~ (((4u & {vs_cbuf9_7_y : 0}) == 4u) ? 4294967295u : 0u)) | (~ (((({in_attr7.z : 0.58835} > 0.5f) && (! isnan({in_attr7.z : 0.58835}))) && (! isnan(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
<=>
if(((! (((~ (((4u & vs_cbuf9_7_y) == 4u) ? 4294967295u : 0u)) | (~ ((((in_attr7.z > 0.5f) && (! isnan(in_attr7.z))) && (! isnan(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
<=>if(b_4_1...)
*/
	{
		f_20_1 = 0.f - (f_8_9 );	// -0.50196
		pf_15_2 = f_20_1 + 1.f;	// 0.49804
		u_21_3 = ftou(pf_15_2 );	// 1056898842
		u_20_4 = u_21_3;	/* 1056898842  <=>  {ftou(((0.f - {in_attr1.x : 0.50196}) + 1.f)) : 1056898842}
<=>
ftou(((0.f - in_attr1.x) + 1.f))
<=>
{u_20_4 = 
	{u_21_3 = ftou(
		{pf_15_2 = (
			{f_20_1 = (0.f - 
				{f_8_9 = in_attr1.x
				})
			} + 1.f)
		})
	}
}
*/
		f_20_phi_24 = f_20_1;
		pf_15_phi_24 = pf_15_2;
		u_21_phi_24 = u_21_3;
		u_20_phi_24 = u_20_4;
	} 
	f_20_2 = utof(u_11_1 );	// 0.9999999
	pf_15_3 = f_20_2 * f_16_4;	// 0
	u_9_18 = ~ u_9_17;	// 4294967295
	u_8_12 = ~ u_8_11;	// 0
	u_8_13 = u_9_18 | u_8_12;	// 4294967295
	b_3_8 = u_8_13 != 0u;	// True
	f_16_5 = trunc(pf_15_3 );	// 0
	f_16_6 = min(max(f_16_5, float(0.f ) ), float(4294967300.f ) );	/* 0  <=>  clamp(trunc(({utof(({ftou((1.0f / float({u_6_7 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999} * float({u_8_10 : 0}))), float(0.f), float(4294967300.f))
<=>
clamp(trunc((utof((ftou((1.0f / float(u_6_7))) + 4294967294u)) * float(u_8_10))), float(0.f), float(4294967300.f))
<=>
{f_16_6 = clamp(
	{f_16_5 = trunc(
		{pf_15_3 = (
			{f_20_2 = utof(
				{u_11_1 = (
					{u_11_0 = ftou(
						{f_14_3 = (1.0f / 
							{f_14_2 = float(u_6_7)
							})
						})
					} + 4294967294u)
				})
			} * 
			{f_16_4 = float(u_8_10)
			})
		})
	}, float(0.f), float(4294967300.f))
}
*/
	u_8_14 = uint(f_16_6 );	// 0
	f_16_7 = utof(u_12_1 );	// 0.9999999
	pf_15_4 = f_16_7 * f_17_3;	// 0
	pf_14_13 = pf_14_12 * f_18_2;	// 0.50196
	f_16_8 = trunc(pf_15_4 );	// 0
	f_16_9 = min(max(f_16_8, float(0.f ) ), float(4294967300.f ) );	// 0
	u_9_19 = uint(f_16_9 );	// 0
	f_16_10 = utof(vs_cbuf9[83 ].x  );	// 1.00
	f_17_4 = utof(u_20_phi_24 );	// 0.50196
	pf_15_5 = f_17_4 * f_16_10;	// 0.50196
	out_attr2.w  = pf_14_13;	/* 0.50196  <=>  (({utof(u_20_phi_23) : 0.50196} * {utof(vs_cbuf9[83].y) : 1.00}) * (1.0f / {utof(vs_cbuf9[83].w) : 1.00}))
<=>
((utof(u_20_phi_23) * utof(vs_cbuf9[83].y)) * (1.0f / utof(vs_cbuf9[83].w)))
<=>
{out_attr2.w = 
	{pf_14_13 = (
		{pf_14_12 = (
			{f_20_0 = utof(u_20_phi_23)
			} * 
			{f_19_3 = utof(vs_cbuf9[83].y)
			})
		} * 
		{f_18_2 = (1.0f / 
			{f_18_1 = utof(vs_cbuf9[83].w)
			})
		})
	}
}
*/
	b_4_2 = ! b_2_6;	// True
	b_6_4 = b_4_2 ? (true) : (false);	// True
	u_11_2 = u_15_6;	/* 0  <=>  {ftou(in_attr2.x) : 0}
<=>
ftou(in_attr2.x)
<=>
{u_11_2 = 
	{u_15_6 = ftou(
		{f_13_12 = in_attr2.x
		})
	}
}
*/
	f_16_phi_25 = f_16_10;
	pf_14_phi_25 = pf_14_13;
	u_12_phi_25 = u_12_1;
	u_11_phi_25 = u_11_2;
	if(b_6_4 )	/* True  <=>  if(((! (((({in_attr0.z : 0.001} == 0.f) && (! isnan({in_attr0.z : 0.001}))) && (! isnan(0.f))) && (((({in_attr0.x : 0} == 0.f) && (! isnan({in_attr0.x : 0}))) && (! isnan(0.f))) && ((({in_attr0.y : 0} == 0.f) && (! isnan({in_attr0.y : 0}))) && (! isnan(0.f)))))) ? true : false))
<=>
if(((! ((((in_attr0.z == 0.f) && (! isnan(in_attr0.z))) && (! isnan(0.f))) && ((((in_attr0.x == 0.f) && (! isnan(in_attr0.x))) && (! isnan(0.f))) && (((in_attr0.y == 0.f) && (! isnan(in_attr0.y))) && (! isnan(0.f)))))) ? true : false))
<=>if(b_6_4...)
*/
	{
		f_16_11 = utof(vs_cbuf13[0 ].x  );	// 0
		pf_14_14 = fma(f_10_2, f_16_11, f_13_12 );	// 0
		u_12_2 = ftou(pf_14_14 );	// 0
		u_11_3 = u_12_2;	/* 0  <=>  {ftou((({in_attr0.x : 0} * {(vs_cbuf13_0.x) : 0}) + {in_attr2.x : 0})) : 0}
<=>
ftou(((in_attr0.x * (vs_cbuf13_0.x)) + in_attr2.x))
<=>
{u_11_3 = 
	{u_12_2 = ftou(
		{pf_14_14 = ((
			{f_10_2 = in_attr0.x
			} * 
			{f_16_11 = (vs_cbuf13_0.x)
			}) + 
			{f_13_12 = in_attr2.x
			})
		})
	}
}
*/
		f_16_phi_25 = f_16_11;
		pf_14_phi_25 = pf_14_14;
		u_12_phi_25 = u_12_2;
		u_11_phi_25 = u_11_3;
	} 
	b_4_3 = ! b_2_6;	// True
	b_6_5 = b_4_3 ? (true) : (false);	// True
	u_12_3 = u_19_1;	/* 1065353216  <=>  {ftou(in_attr2.z) : 1065353216}
<=>
ftou(in_attr2.z)
<=>
{u_12_3 = 
	{u_19_1 = ftou(
		{f_11_5 = in_attr2.z
		})
	}
}
*/
	f_10_phi_26 = f_10_2;
	pf_14_phi_26 = pf_14_phi_25;
	u_15_phi_26 = u_15_6;
	u_12_phi_26 = u_12_3;
	if(b_6_5 )	/* True  <=>  if(((! (((({in_attr0.z : 0.001} == 0.f) && (! isnan({in_attr0.z : 0.001}))) && (! isnan(0.f))) && (((({in_attr0.x : 0} == 0.f) && (! isnan({in_attr0.x : 0}))) && (! isnan(0.f))) && ((({in_attr0.y : 0} == 0.f) && (! isnan({in_attr0.y : 0}))) && (! isnan(0.f)))))) ? true : false))
<=>
if(((! ((((in_attr0.z == 0.f) && (! isnan(in_attr0.z))) && (! isnan(0.f))) && ((((in_attr0.x == 0.f) && (! isnan(in_attr0.x))) && (! isnan(0.f))) && (((in_attr0.y == 0.f) && (! isnan(in_attr0.y))) && (! isnan(0.f)))))) ? true : false))
<=>if(b_6_5...)
*/
	{
		f_10_3 = utof(vs_cbuf13[0 ].x  );	// 0
		pf_14_15 = fma(f_1_6, f_10_3, f_11_5 );	// 1.00
		u_15_7 = ftou(pf_14_15 );	// 1065353216
		u_12_4 = u_15_7;	/* 1065353216  <=>  {ftou((({in_attr0.z : 0.001} * {(vs_cbuf13_0.x) : 0}) + {in_attr2.z : 1.00})) : 1065353216}
<=>
ftou(((in_attr0.z * (vs_cbuf13_0.x)) + in_attr2.z))
<=>
{u_12_4 = 
	{u_15_7 = ftou(
		{pf_14_15 = ((
			{f_1_6 = in_attr0.z
			} * 
			{f_10_3 = (vs_cbuf13_0.x)
			}) + 
			{f_11_5 = in_attr2.z
			})
		})
	}
}
*/
		f_10_phi_26 = f_10_3;
		pf_14_phi_26 = pf_14_15;
		u_15_phi_26 = u_15_7;
		u_12_phi_26 = u_12_4;
	} 
	pf_14_16 = pf_15_5 * f_19_5;	// 0.50196
	out_attr2.z  = pf_14_16;	/* 0.50196  <=>  (({utof(u_20_phi_24) : 0.50196} * {utof(vs_cbuf9[83].x) : 1.00}) * (1.0f / {utof(vs_cbuf9[83].z) : 1.00}))
<=>
((utof(u_20_phi_24) * utof(vs_cbuf9[83].x)) * (1.0f / utof(vs_cbuf9[83].z)))
<=>
{out_attr2.z = 
	{pf_14_16 = (
		{pf_15_5 = (
			{f_17_4 = utof(u_20_phi_24)
			} * 
			{f_16_10 = utof(vs_cbuf9[83].x)
			})
		} * 
		{f_19_5 = (1.0f / 
			{f_19_4 = utof(vs_cbuf9[83].z)
			})
		})
	}
}
*/
	f_1_7 = utof(vs_cbuf10[0 ].x  );	// 1.00
	f_10_4 = utof(vs_cbuf9[105 ].x  );	// 0.8475056
	pf_14_17 = f_10_4 * f_1_7;	// 0.8475056
	u_8_15 = u_16_0 + u_8_14;	/* 0  <=>  ({u_16_0 : 0} + uint({f_16_6 : 0}))
<=>
(u_16_0 + uint(f_16_6))
<=>
{u_8_15 = (u_16_0 + 
	{u_8_14 = uint(f_16_6)
	})
}
*/
	f_1_8 = utof(vs_cbuf10[0 ].y  );	// 1.00
	f_10_5 = utof(vs_cbuf9[105 ].y  );	// 0.9126984
	pf_15_6 = f_10_5 * f_1_8;	// 0.9126984
	u_9_20 = u_17_0 + u_9_19;	/* 0  <=>  ({u_17_0 : 0} + uint(clamp(trunc(({utof(u_12_1) : 0.9999999} * float({u_9_14 : 0}))), float(0.f), float(4294967300.f))))
<=>
(u_17_0 + uint(clamp(trunc((utof(u_12_1) * float(u_9_14))), float(0.f), float(4294967300.f))))
<=>
{u_9_20 = (u_17_0 + 
	{u_9_19 = uint(
		{f_16_9 = clamp(
			{f_16_8 = trunc(
				{pf_15_4 = (
					{f_16_7 = utof(u_12_1)
					} * 
					{f_17_3 = float(u_9_14)
					})
				})
			}, float(0.f), float(4294967300.f))
		})
	})
}
*/
	b_2_7 = ! b_2_6;	// True
	b_4_4 = b_2_7 ? (true) : (false);	// True
	u_15_8 = u_14_5;	/* 0  <=>  {ftou(in_attr2.y) : 0}
<=>
ftou(in_attr2.y)
<=>
{u_15_8 = 
	{u_14_5 = ftou(
		{f_9_4 = in_attr2.y
		})
	}
}
*/
	f_1_phi_27 = f_1_8;
	pf_16_phi_27 = pf_16_3;
	u_16_phi_27 = u_16_0;
	u_15_phi_27 = u_15_8;
	if(b_4_4 )	/* True  <=>  if(((! (((({in_attr0.z : 0.001} == 0.f) && (! isnan({in_attr0.z : 0.001}))) && (! isnan(0.f))) && (((({in_attr0.x : 0} == 0.f) && (! isnan({in_attr0.x : 0}))) && (! isnan(0.f))) && ((({in_attr0.y : 0} == 0.f) && (! isnan({in_attr0.y : 0}))) && (! isnan(0.f)))))) ? true : false))
<=>
if(((! ((((in_attr0.z == 0.f) && (! isnan(in_attr0.z))) && (! isnan(0.f))) && ((((in_attr0.x == 0.f) && (! isnan(in_attr0.x))) && (! isnan(0.f))) && (((in_attr0.y == 0.f) && (! isnan(in_attr0.y))) && (! isnan(0.f)))))) ? true : false))
<=>if(b_4_4...)
*/
	{
		f_1_9 = utof(vs_cbuf13[0 ].x  );	// 0
		pf_16_4 = fma(f_12_3, f_1_9, f_9_4 );	// 0
		u_16_1 = ftou(pf_16_4 );	// 0
		u_15_9 = u_16_1;	/* 0  <=>  {ftou((({in_attr0.y : 0} * {(vs_cbuf13_0.x) : 0}) + {in_attr2.y : 0})) : 0}
<=>
ftou(((in_attr0.y * (vs_cbuf13_0.x)) + in_attr2.y))
<=>
{u_15_9 = 
	{u_16_1 = ftou(
		{pf_16_4 = ((
			{f_12_3 = in_attr0.y
			} * 
			{f_1_9 = (vs_cbuf13_0.x)
			}) + 
			{f_9_4 = in_attr2.y
			})
		})
	}
}
*/
		f_1_phi_27 = f_1_9;
		pf_16_phi_27 = pf_16_4;
		u_16_phi_27 = u_16_1;
		u_15_phi_27 = u_15_9;
	} 
	f_1_10 = utof(vs_cbuf10[0 ].w  );	// 1.00
	f_9_5 = utof(vs_cbuf9[113 ].x  );	// 1.50
	pf_16_5 = f_9_5 * f_1_10;	// 1.50
	out_attr0.w  = pf_16_5;	/* 1.50  <=>  ({utof(vs_cbuf9[113].x) : 1.50} * {(vs_cbuf10_0.w) : 1.00})
<=>
(utof(vs_cbuf9[113].x) * (vs_cbuf10_0.w))
<=>
{out_attr0.w = 
	{pf_16_5 = (
		{f_9_5 = utof(vs_cbuf9[113].x)
		} * 
		{f_1_10 = (vs_cbuf10_0.w)
		})
	}
}
*/
	b_2_8 = ! b_5_0;	// False
	b_4_5 = b_2_8 ? (true) : (false);	// False
	if(b_4_5 )	/* False  <=>  if(((! (((~ (((1u & {vs_cbuf9_7_y : 0}) == 1u) ? 4294967295u : 0u)) | (~ (((({in_attr7.x : 0.5484} > 0.5f) && (! isnan({in_attr7.x : 0.5484}))) && (! isnan(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
<=>
if(((! (((~ (((1u & vs_cbuf9_7_y) == 1u) ? 4294967295u : 0u)) | (~ ((((in_attr7.x > 0.5f) && (! isnan(in_attr7.x))) && (! isnan(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
<=>if(b_4_5...)
*/
	{
	} 
	b_2_9 = ! b_3_8;	// False
	b_3_9 = b_2_9 ? (true) : (false);	// False
	u_14_6 = u_13_0;	/* 1056997491  <=>  {ftou(in_attr1.y) : 1056997491}
<=>
ftou(in_attr1.y)
<=>
{u_14_6 = 
	{u_13_0 = ftou(
		{f_14_6 = in_attr1.y
		})
	}
}
*/
	f_1_phi_29 = f_1_10;
	pf_16_phi_29 = pf_16_5;
	u_16_phi_29 = u_16_phi_27;
	u_14_phi_29 = u_14_6;
	if(b_3_9 )	/* False  <=>  if(((! (((~ (((2u & {vs_cbuf9_7_y : 0}) == 2u) ? 4294967295u : 0u)) | (~ (((({in_attr7.y : 0.96051} > 0.5f) && (! isnan({in_attr7.y : 0.96051}))) && (! isnan(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
<=>
if(((! (((~ (((2u & vs_cbuf9_7_y) == 2u) ? 4294967295u : 0u)) | (~ ((((in_attr7.y > 0.5f) && (! isnan(in_attr7.y))) && (! isnan(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
<=>if(b_3_9...)
*/
	{
		f_1_11 = 0.f - (f_14_6 );	// -0.50196
		pf_16_6 = f_1_11 + 1.f;	// 0.49804
		u_16_2 = ftou(pf_16_6 );	// 1056898842
		u_14_7 = u_16_2;	/* 1056898842  <=>  {ftou(((0.f - {in_attr1.y : 0.50196}) + 1.f)) : 1056898842}
<=>
ftou(((0.f - in_attr1.y) + 1.f))
<=>
{u_14_7 = 
	{u_16_2 = ftou(
		{pf_16_6 = (
			{f_1_11 = (0.f - 
				{f_14_6 = in_attr1.y
				})
			} + 1.f)
		})
	}
}
*/
		f_1_phi_29 = f_1_11;
		pf_16_phi_29 = pf_16_6;
		u_16_phi_29 = u_16_2;
		u_14_phi_29 = u_14_7;
	} 
	f_1_12 = utof(vs_cbuf10[0 ].z  );	// 1.00
	f_9_6 = utof(vs_cbuf9[105 ].z  );	// 0.9111589
	pf_16_7 = f_9_6 * f_1_12;	// 0.9111589
	f_1_13 = utof(vs_cbuf10[10 ].x  );	// 0
	f_9_7 = utof(u_11_phi_25 );	// 0
	pf_18_1 = f_9_7 * f_1_13;	// 0
	f_1_14 = utof(vs_cbuf10[9 ].x  );	// 0
	f_9_8 = utof(u_11_phi_25 );	// 0
	pf_19_0 = f_9_8 * f_1_14;	// 0
	f_1_15 = utof(vs_cbuf10[8 ].x  );	// 1.00
	f_9_9 = utof(u_11_phi_25 );	// 0
	pf_20_0 = f_9_9 * f_1_15;	// 0
	u_11_4 = uint(bitfieldExtract(uint(u_6_7 ), int(0u ), int(16u ) ) );	// 1
	u_13_1 = uint(bitfieldExtract(uint(u_8_15 ), int(0u ), int(16u ) ) );	// 0
	u_11_5 = uint(u_11_4 * u_13_1 );	// 0
	u_13_2 = uint(bitfieldExtract(uint(u_6_7 ), int(0u ), int(16u ) ) );	// 1
	u_16_3 = uint(bitfieldExtract(uint(u_8_15 ), int(16u ), int(16u ) ) );	// 0
	u_13_3 = uint(u_13_2 * u_16_3 );	// 0
	u_16_4 = uint(bitfieldExtract(uint(u_8_15 ), int(0u ), int(16u ) ) );	// 0
	u_13_4 = bitfieldInsert(u_13_3, u_16_4, int(16u ), int(16u ) );	// 0
	f_1_16 = utof(vs_cbuf9[104 ].x  );	// 0.30
	pf_15_7 = pf_15_6 * f_1_16;	// 0.2738095
	u_16_5 = uint(bitfieldExtract(uint(u_9_20 ), int(0u ), int(16u ) ) );	// 0
	u_17_1 = uint(bitfieldExtract(uint(u_7_3 ), int(0u ), int(16u ) ) );	// 1
	u_16_6 = uint(u_16_5 * u_17_1 );	// 0
	out_attr0.y  = pf_15_7;	/* 0.2738095  <=>  (({utof(vs_cbuf9[105].y) : 0.9126984} * {(vs_cbuf10_0.y) : 1.00}) * {utof(vs_cbuf9[104].x) : 0.30})
<=>
((utof(vs_cbuf9[105].y) * (vs_cbuf10_0.y)) * utof(vs_cbuf9[104].x))
<=>
{out_attr0.y = 
	{pf_15_7 = (
		{pf_15_6 = (
			{f_10_5 = utof(vs_cbuf9[105].y)
			} * 
			{f_1_8 = (vs_cbuf10_0.y)
			})
		} * 
		{f_1_16 = utof(vs_cbuf9[104].x)
		})
	}
}
*/
	u_17_2 = uint(bitfieldExtract(uint(u_9_20 ), int(0u ), int(16u ) ) );	// 0
	u_19_2 = uint(bitfieldExtract(uint(u_7_3 ), int(16u ), int(16u ) ) );	// 0
	u_17_3 = uint(u_17_2 * u_19_2 );	// 0
	u_19_3 = uint(bitfieldExtract(uint(u_7_3 ), int(0u ), int(16u ) ) );	// 1
	u_17_4 = bitfieldInsert(u_17_3, u_19_3, int(16u ), int(16u ) );	// 65536
	f_1_17 = utof(vs_cbuf9[104 ].x  );	// 0.30
	pf_14_18 = pf_14_17 * f_1_17;	// 0.2542517
	f_1_18 = utof(vs_cbuf10[10 ].y  );	// 0
	f_9_10 = utof(u_12_phi_26 );	// 1.00
	pf_15_8 = fma(f_9_10, f_1_18, pf_18_1 );	// 0
	out_attr0.x  = pf_14_18;	/* 0.2542517  <=>  (({utof(vs_cbuf9[105].x) : 0.8475056} * {(vs_cbuf10_0.x) : 1.00}) * {utof(vs_cbuf9[104].x) : 0.30})
<=>
((utof(vs_cbuf9[105].x) * (vs_cbuf10_0.x)) * utof(vs_cbuf9[104].x))
<=>
{out_attr0.x = 
	{pf_14_18 = (
		{pf_14_17 = (
			{f_10_4 = utof(vs_cbuf9[105].x)
			} * 
			{f_1_7 = (vs_cbuf10_0.x)
			})
		} * 
		{f_1_17 = utof(vs_cbuf9[104].x)
		})
	}
}
*/
	f_1_19 = utof(vs_cbuf10[9 ].y  );	// 1.00
	f_9_11 = utof(u_12_phi_26 );	// 1.00
	pf_14_19 = fma(f_9_11, f_1_19, pf_19_0 );	// 1.00
	f_1_20 = utof(vs_cbuf10[1 ].x  );	// 1.00
	f_9_12 = utof(vs_cbuf9[121 ].x  );	// 0.4303351
	pf_18_2 = f_9_12 * f_1_20;	// 0.4303351
	f_1_21 = utof(vs_cbuf10[8 ].y  );	// 0
	f_9_13 = utof(u_12_phi_26 );	// 1.00
	pf_19_1 = fma(f_9_13, f_1_21, pf_20_0 );	// 0
	u_12_5 = uint(bitfieldExtract(uint(u_6_7 ), int(16u ), int(16u ) ) );	// 0
	u_19_4 = uint(bitfieldExtract(uint(u_13_4 ), int(16u ), int(16u ) ) );	/* 0  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_6_7 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_8_15 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_8_15 : 0}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
<=>
uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_6_7), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_8_15), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_8_15), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
<=>
{u_19_4 = uint(bitfieldExtract(uint(
	{u_13_4 = bitfieldInsert(
		{u_13_3 = uint((
			{u_13_2 = uint(bitfieldExtract(uint(u_6_7), int(0u), int(16u)))
			} * 
			{u_16_3 = uint(bitfieldExtract(uint(u_8_15), int(16u), int(16u)))
			}))
		}, 
		{u_16_4 = uint(bitfieldExtract(uint(u_8_15), int(0u), int(16u)))
		}, int(16u), int(16u))
	}), int(16u), int(16u)))
}
*/
	u_12_6 = uint(u_12_5 * u_19_4 );	// 0
	u_12_7 = u_12_6 << 16u;	// 0
	u_13_5 = u_13_4 << 16u;	// 0
	u_11_6 = u_13_5 + u_11_5;	/* 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_6_7 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_8_15 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_8_15 : 0}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_6_7 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_8_15 : 0}), int(0u), int(16u))))))
<=>
((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_6_7), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_8_15), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_8_15), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_6_7), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_8_15), int(0u), int(16u))))))
<=>
{u_11_6 = (
	{u_13_5 = (
		{u_13_4 = bitfieldInsert(
			{u_13_3 = uint((
				{u_13_2 = uint(bitfieldExtract(uint(u_6_7), int(0u), int(16u)))
				} * 
				{u_16_3 = uint(bitfieldExtract(uint(u_8_15), int(16u), int(16u)))
				}))
			}, 
			{u_16_4 = uint(bitfieldExtract(uint(u_8_15), int(0u), int(16u)))
			}, int(16u), int(16u))
		} << 16u)
	} + 
	{u_11_5 = uint((
		{u_11_4 = uint(bitfieldExtract(uint(u_6_7), int(0u), int(16u)))
		} * 
		{u_13_1 = uint(bitfieldExtract(uint(u_8_15), int(0u), int(16u)))
		}))
	})
}
*/
	u_11_7 = u_12_7 + u_11_6;	// 0
	u_12_8 = uint(bitfieldExtract(uint(u_9_20 ), int(16u ), int(16u ) ) );	// 0
	u_13_6 = uint(bitfieldExtract(uint(u_17_4 ), int(16u ), int(16u ) ) );	/* 1  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_9_20 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_7_3 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_7_3 : 1}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
<=>
uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_9_20), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_7_3), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_7_3), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
<=>
{u_13_6 = uint(bitfieldExtract(uint(
	{u_17_4 = bitfieldInsert(
		{u_17_3 = uint((
			{u_17_2 = uint(bitfieldExtract(uint(u_9_20), int(0u), int(16u)))
			} * 
			{u_19_2 = uint(bitfieldExtract(uint(u_7_3), int(16u), int(16u)))
			}))
		}, 
		{u_19_3 = uint(bitfieldExtract(uint(u_7_3), int(0u), int(16u)))
		}, int(16u), int(16u))
	}), int(16u), int(16u)))
}
*/
	u_12_9 = uint(u_12_8 * u_13_6 );	// 0
	u_12_10 = u_12_9 << 16u;	// 0
	u_13_7 = u_17_4 << 16u;	/* 0  <=>  (bitfieldInsert(uint((uint(bitfieldExtract(uint({u_9_20 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_7_3 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_7_3 : 1}), int(0u), int(16u))), int(16u), int(16u)) << 16u)
<=>
(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_9_20), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_7_3), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_7_3), int(0u), int(16u))), int(16u), int(16u)) << 16u)
<=>
{u_13_7 = (
	{u_17_4 = bitfieldInsert(
		{u_17_3 = uint((
			{u_17_2 = uint(bitfieldExtract(uint(u_9_20), int(0u), int(16u)))
			} * 
			{u_19_2 = uint(bitfieldExtract(uint(u_7_3), int(16u), int(16u)))
			}))
		}, 
		{u_19_3 = uint(bitfieldExtract(uint(u_7_3), int(0u), int(16u)))
		}, int(16u), int(16u))
	} << 16u)
}
*/
	u_13_8 = u_13_7 + u_16_6;	// 0
	u_12_11 = u_12_10 + u_13_8;	/* 0  <=>  ((uint((uint(bitfieldExtract(uint({u_9_20 : 0}), int(16u), int(16u))) * {u_13_6 : 1})) << 16u) + ({u_13_7 : 0} + uint((uint(bitfieldExtract(uint({u_9_20 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_7_3 : 1}), int(0u), int(16u)))))))
<=>
((uint((uint(bitfieldExtract(uint(u_9_20), int(16u), int(16u))) * u_13_6)) << 16u) + (u_13_7 + uint((uint(bitfieldExtract(uint(u_9_20), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_7_3), int(0u), int(16u)))))))
<=>
{u_12_11 = (
	{u_12_10 = (
		{u_12_9 = uint((
			{u_12_8 = uint(bitfieldExtract(uint(u_9_20), int(16u), int(16u)))
			} * u_13_6))
		} << 16u)
	} + 
	{u_13_8 = (u_13_7 + 
		{u_16_6 = uint((
			{u_16_5 = uint(bitfieldExtract(uint(u_9_20), int(0u), int(16u)))
			} * 
			{u_17_1 = uint(bitfieldExtract(uint(u_7_3), int(0u), int(16u)))
			}))
		})
	})
}
*/
	f_1_22 = utof(vs_cbuf10[10 ].z  );	// 1.00
	f_9_14 = utof(u_15_phi_27 );	// 0
	f_1_23 = 0.f - (f_1_22 );	// -1
	pf_15_9 = fma(f_9_14, f_1_23, pf_15_8 );	// 0
	f_1_24 = in_attr3.y ;	// 1.00
	f_9_15 = utof(vs_cbuf10[9 ].z  );	// 0
	f_10_6 = utof(u_15_phi_27 );	// 0
	f_9_16 = 0.f - (f_9_15 );	// 0
	pf_14_20 = fma(f_10_6, f_9_16, pf_14_19 );	// 1.00
	b_2_10 = ! b_5_0;	// False
	b_3_10 = b_2_10 ? (true) : (false);	// False
	u_13_9 = u_18_0;	/* 1056997491  <=>  {ftou(in_attr1.x) : 1056997491}
<=>
ftou(in_attr1.x)
<=>
{u_13_9 = 
	{u_18_0 = ftou(
		{f_8_9 = in_attr1.x
		})
	}
}
*/
	f_8_phi_30 = f_8_9;
	pf_20_phi_30 = pf_20_0;
	u_16_phi_30 = u_16_6;
	u_13_phi_30 = u_13_9;
	if(b_3_10 )	/* False  <=>  if(((! (((~ (((1u & {vs_cbuf9_7_y : 0}) == 1u) ? 4294967295u : 0u)) | (~ (((({in_attr7.x : 0.5484} > 0.5f) && (! isnan({in_attr7.x : 0.5484}))) && (! isnan(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
<=>
if(((! (((~ (((1u & vs_cbuf9_7_y) == 1u) ? 4294967295u : 0u)) | (~ ((((in_attr7.x > 0.5f) && (! isnan(in_attr7.x))) && (! isnan(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
<=>if(b_3_10...)
*/
	{
		f_8_10 = 0.f - (f_8_9 );	// -0.50196
		pf_20_1 = f_8_10 + 1.f;	// 0.49804
		u_16_7 = ftou(pf_20_1 );	// 1056898842
		u_13_10 = u_16_7;	/* 1056898842  <=>  {ftou(((0.f - {in_attr1.x : 0.50196}) + 1.f)) : 1056898842}
<=>
ftou(((0.f - in_attr1.x) + 1.f))
<=>
{u_13_10 = 
	{u_16_7 = ftou(
		{pf_20_1 = (
			{f_8_10 = (0.f - 
				{f_8_9 = in_attr1.x
				})
			} + 1.f)
		})
	}
}
*/
		f_8_phi_30 = f_8_10;
		pf_20_phi_30 = pf_20_1;
		u_16_phi_30 = u_16_7;
		u_13_phi_30 = u_13_10;
	} 
	f_8_11 = utof(vs_cbuf9[104 ].x  );	// 0.30
	pf_16_8 = pf_16_7 * f_8_11;	// 0.2733477
	f_8_12 = utof(vs_cbuf9[104 ].x  );	// 0.30
	pf_18_3 = pf_18_2 * f_8_12;	// 0.1291005
	out_attr0.z  = pf_16_8;	/* 0.2733477  <=>  (({utof(vs_cbuf9[105].z) : 0.9111589} * {(vs_cbuf10_0.z) : 1.00}) * {utof(vs_cbuf9[104].x) : 0.30})
<=>
((utof(vs_cbuf9[105].z) * (vs_cbuf10_0.z)) * utof(vs_cbuf9[104].x))
<=>
{out_attr0.z = 
	{pf_16_8 = (
		{pf_16_7 = (
			{f_9_6 = utof(vs_cbuf9[105].z)
			} * 
			{f_1_12 = (vs_cbuf10_0.z)
			})
		} * 
		{f_8_11 = utof(vs_cbuf9[104].x)
		})
	}
}
*/
	f_8_13 = utof(vs_cbuf10[8 ].z  );	// 0
	f_9_17 = utof(u_15_phi_27 );	// 0
	f_8_14 = 0.f - (f_8_13 );	// 0
	pf_16_9 = fma(f_9_17, f_8_14, pf_19_1 );	// 0
	out_attr1.x  = pf_18_3;	/* 0.1291005  <=>  (({utof(vs_cbuf9[121].x) : 0.4303351} * {(vs_cbuf10_1.x) : 1.00}) * {utof(vs_cbuf9[104].x) : 0.30})
<=>
((utof(vs_cbuf9[121].x) * (vs_cbuf10_1.x)) * utof(vs_cbuf9[104].x))
<=>
{out_attr1.x = 
	{pf_18_3 = (
		{pf_18_2 = (
			{f_9_12 = utof(vs_cbuf9[121].x)
			} * 
			{f_1_20 = (vs_cbuf10_1.x)
			})
		} * 
		{f_8_12 = utof(vs_cbuf9[104].x)
		})
	}
}
*/
	u_11_8 = uint(int(0 ) - int(u_11_7 ) );	/* 0  <=>  uint((int(0) - int(((uint((uint(bitfieldExtract(uint({u_6_7 : 1}), int(16u), int(16u))) * {u_19_4 : 0})) << 16u) + {u_11_6 : 0}))))
<=>
uint((int(0) - int(((uint((uint(bitfieldExtract(uint(u_6_7), int(16u), int(16u))) * u_19_4)) << 16u) + u_11_6))))
<=>
{u_11_8 = uint((int(0) - int(
	{u_11_7 = (
		{u_12_7 = (
			{u_12_6 = uint((
				{u_12_5 = uint(bitfieldExtract(uint(u_6_7), int(16u), int(16u)))
				} * u_19_4))
			} << 16u)
		} + u_11_6)
	})))
}
*/
	f_8_15 = in_attr3.z ;	// 1.00
	u_12_12 = uint(int(0 ) - int(u_12_11 ) );	/* 0  <=>  uint((int(0) - int({u_12_11 : 0})))
<=>
uint((int(0) - int(u_12_11)))
<=>
{u_12_12 = uint((int(0) - int(u_12_11)))
}
*/
	f_9_18 = in_attr3.w ;	// 1.00
	b_0_8 = ! b_0_7;	/* True  <=>  (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(uint({u_6_7 : 1}), int(0u), int(32u)))), int(0u), int(32u))) == 0))
<=>
(! (uint(bitfieldExtract(uint(uint(bitfieldExtract(uint(u_6_7), int(0u), int(32u)))), int(0u), int(32u))) == 0))
<=>
{b_0_8 = (! 
	{b_0_7 = (
		{u_10_1 = uint(bitfieldExtract(uint(
			{u_10_0 = uint(bitfieldExtract(uint(u_6_7), int(0u), int(32u)))
			}), int(0u), int(32u)))
		} == 0)
	})
}
*/
	b_0_9 = b_1_22 || b_0_8;	// True
	u_15_10 = uint(bitfieldExtract(int(u_6_7 ), int(0u ), int(32u ) ) );	// 1
	u_15_11 = uint(bitfieldExtract(uint(u_15_10 ), int(0u ), int(32u ) ) );	// 1
	b_1_23 = u_15_11 == 0;	// False
	b_2_11 = int(u_15_11 ) < 0;	/* False  <=>  (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(int({u_6_7 : 1}), int(0u), int(32u)))), int(0u), int(32u)))) < 0)
<=>
(int(uint(bitfieldExtract(uint(uint(bitfieldExtract(int(u_6_7), int(0u), int(32u)))), int(0u), int(32u)))) < 0)
<=>
{b_2_11 = (int(
	{u_15_11 = uint(bitfieldExtract(uint(
		{u_15_10 = uint(bitfieldExtract(int(u_6_7), int(0u), int(32u)))
		}), int(0u), int(32u)))
	}) < 0)
}
*/
	f_10_7 = utof(vs_cbuf15[27 ].z  );	// 250.00
	f_11_6 = utof(vs_cbuf8[29 ].y  );	// 365.7373
	f_10_8 = min(f_11_6, f_10_7 );	// 250.00
	out_attr5.y  = f_1_24;	/* 1.00  <=>  {in_attr3.y : 1.00}
<=>
in_attr3.y
<=>
{out_attr5.y = 
	{f_1_24 = in_attr3.y
	}
}
*/
	f_1_25 = utof(vs_cbuf10[1 ].y  );	// 1.00
	f_11_7 = utof(vs_cbuf9[121 ].y  );	// 0.4726913
	pf_18_4 = f_11_7 * f_1_25;	// 0.4726913
	f_1_26 = utof(vs_cbuf10[1 ].z  );	// 1.00
	f_11_8 = utof(vs_cbuf9[121 ].z  );	// 0.484127
	pf_19_2 = f_11_8 * f_1_26;	// 0.484127
	b_3_11 = uint(u_11_8 ) >= uint(u_6_7 );	// False
	u_11_9 = b_3_11 ? (4294967295u) : (0u);	/* 0  <=>  ((uint({u_11_8 : 0}) >= uint({u_6_7 : 1})) ? 4294967295u : 0u)
<=>
((uint(u_11_8) >= uint(u_6_7)) ? 4294967295u : 0u)
<=>
{u_11_9 = (
	{b_3_11 = (uint(u_11_8) >= uint(u_6_7))
	} ? 4294967295u : 0u)
}
*/
	b_3_12 = uint(u_12_12 ) >= uint(u_7_3 );	// False
	u_7_4 = b_3_12 ? (4294967295u) : (0u);	// 0
	u_12_13 = u_6_7 >> 31u;	// 0
	out_attr5.z  = f_8_15;	/* 1.00  <=>  {in_attr3.z : 1.00}
<=>
in_attr3.z
<=>
{out_attr5.z = 
	{f_8_15 = in_attr3.z
	}
}
*/
	pf_3_9 = fma(0.f, pf_3_8, pf_15_9 );	// 0
	out_attr5.w  = f_9_18;	/* 1.00  <=>  {in_attr3.w : 1.00}
<=>
in_attr3.w
<=>
{out_attr5.w = 
	{f_9_18 = in_attr3.w
	}
}
*/
	u_8_16 = uint(int(0 ) - int(u_8_15 ) );	// 0
	u_8_17 = u_8_16 + u_11_9;	/* 0  <=>  (uint((int(0) - int({u_8_15 : 0}))) + {u_11_9 : 0})
<=>
(uint((int(0) - int(u_8_15))) + u_11_9)
<=>
{u_8_17 = (
	{u_8_16 = uint((int(0) - int(u_8_15)))
	} + u_11_9)
}
*/
	out_attr6.z  = pf_3_9;	/* 0  <=>  ((0.f * ((({pf_7_1 : 0} * {(vs_cbuf10_6.z) : 1.00}) + (({pf_6_1 : 0} * {(vs_cbuf10_6.y) : 0}) + ({pf_3_4 : 0} * {(vs_cbuf10_6.x) : 0}))) + {(vs_cbuf10_6.w) : -3936.7583})) + (({utof(u_15_phi_27) : 0} * (0.f - {(vs_cbuf10_10.z) : 1.00})) + (({utof(u_12_phi_26) : 1.00} * {(vs_cbuf10_10.y) : 0}) + ({utof(u_11_phi_25) : 0} * {(vs_cbuf10_10.x) : 0}))))
<=>
((0.f * (((pf_7_1 * (vs_cbuf10_6.z)) + ((pf_6_1 * (vs_cbuf10_6.y)) + (pf_3_4 * (vs_cbuf10_6.x)))) + (vs_cbuf10_6.w))) + ((utof(u_15_phi_27) * (0.f - (vs_cbuf10_10.z))) + ((utof(u_12_phi_26) * (vs_cbuf10_10.y)) + (utof(u_11_phi_25) * (vs_cbuf10_10.x)))))
<=>
{out_attr6.z = 
	{pf_3_9 = ((0.f * 
		{pf_3_8 = (
			{pf_3_7 = ((pf_7_1 * 
				{f_2_20 = (vs_cbuf10_6.z)
				}) + 
				{pf_3_6 = ((pf_6_1 * 
					{f_2_13 = (vs_cbuf10_6.y)
					}) + 
					{pf_3_5 = (pf_3_4 * 
						{f_2_8 = (vs_cbuf10_6.x)
						})
					})
				})
			} + 
			{f_2_25 = (vs_cbuf10_6.w)
			})
		}) + 
		{pf_15_9 = ((
			{f_9_14 = utof(u_15_phi_27)
			} * 
			{f_1_23 = (0.f - 
				{f_1_22 = (vs_cbuf10_10.z)
				})
			}) + 
			{pf_15_8 = ((
				{f_9_10 = utof(u_12_phi_26)
				} * 
				{f_1_18 = (vs_cbuf10_10.y)
				}) + 
				{pf_18_1 = (
					{f_9_7 = utof(u_11_phi_25)
					} * 
					{f_1_13 = (vs_cbuf10_10.x)
					})
				})
			})
		})
	}
}
*/
	u_7_5 = uint(int(0 ) - int(u_7_4 ) );	// 0
	u_7_6 = u_9_20 + u_7_5;	// 0
	u_9_21 = uint(int(0 ) - int(u_12_13 ) );	/* 0  <=>  uint((int(0) - int(({u_6_7 : 1} >> 31u))))
<=>
uint((int(0) - int((u_6_7 >> 31u))))
<=>
{u_9_21 = uint((int(0) - int(
	{u_12_13 = (u_6_7 >> 31u)
	})))
}
*/
	u_11_10 = uint(bitfieldExtract(uint(u_6_7 ), int(0u ), int(16u ) ) );	// 1
	u_12_14 = uint(bitfieldExtract(uint(u_8_17 ), int(0u ), int(16u ) ) );	// 0
	u_11_11 = uint(u_11_10 * u_12_14 );	// 0
	u_12_15 = uint(bitfieldExtract(uint(u_6_7 ), int(0u ), int(16u ) ) );	// 1
	u_16_8 = uint(bitfieldExtract(uint(u_8_17 ), int(16u ), int(16u ) ) );	/* 0  <=>  uint(bitfieldExtract(uint({u_8_17 : 0}), int(16u), int(16u)))
<=>
uint(bitfieldExtract(uint(u_8_17), int(16u), int(16u)))
<=>
{u_16_8 = uint(bitfieldExtract(uint(u_8_17), int(16u), int(16u)))
}
*/
	u_12_16 = uint(u_12_15 * u_16_8 );	// 0
	u_8_18 = uint(bitfieldExtract(uint(u_8_17 ), int(0u ), int(16u ) ) );	// 0
	u_8_19 = bitfieldInsert(u_12_16, u_8_18, int(16u ), int(16u ) );	// 0
	u_7_7 = u_7_6 ^ u_9_21;	// 0
	b_1_24 = ! b_1_23;	/* True  <=>  (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(int({u_6_7 : 1}), int(0u), int(32u)))), int(0u), int(32u))) == 0))
<=>
(! (uint(bitfieldExtract(uint(uint(bitfieldExtract(int(u_6_7), int(0u), int(32u)))), int(0u), int(32u))) == 0))
<=>
{b_1_24 = (! 
	{b_1_23 = (
		{u_15_11 = uint(bitfieldExtract(uint(
			{u_15_10 = uint(bitfieldExtract(int(u_6_7), int(0u), int(32u)))
			}), int(0u), int(32u)))
		} == 0)
	})
}
*/
	b_1_25 = b_2_11 || b_1_24;	// True
	u_6_8 = uint(bitfieldExtract(uint(u_6_7 ), int(16u ), int(16u ) ) );	// 0
	u_12_17 = uint(bitfieldExtract(uint(u_8_19 ), int(16u ), int(16u ) ) );	/* 0  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_6_7 : 1}), int(0u), int(16u))) * {u_16_8 : 0})), uint(bitfieldExtract(uint({u_8_17 : 0}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
<=>
uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_6_7), int(0u), int(16u))) * u_16_8)), uint(bitfieldExtract(uint(u_8_17), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
<=>
{u_12_17 = uint(bitfieldExtract(uint(
	{u_8_19 = bitfieldInsert(
		{u_12_16 = uint((
			{u_12_15 = uint(bitfieldExtract(uint(u_6_7), int(0u), int(16u)))
			} * u_16_8))
		}, 
		{u_8_18 = uint(bitfieldExtract(uint(u_8_17), int(0u), int(16u)))
		}, int(16u), int(16u))
	}), int(16u), int(16u)))
}
*/
	u_6_9 = uint(u_6_8 * u_12_17 );	// 0
	u_6_10 = u_6_9 << 16u;	// 0
	u_8_20 = u_8_19 << 16u;	// 0
	u_8_21 = u_8_20 + u_11_11;	/* 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_6_7 : 1}), int(0u), int(16u))) * {u_16_8 : 0})), uint(bitfieldExtract(uint({u_8_17 : 0}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_6_7 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_8_17 : 0}), int(0u), int(16u))))))
<=>
((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_6_7), int(0u), int(16u))) * u_16_8)), uint(bitfieldExtract(uint(u_8_17), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_6_7), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_8_17), int(0u), int(16u))))))
<=>
{u_8_21 = (
	{u_8_20 = (
		{u_8_19 = bitfieldInsert(
			{u_12_16 = uint((
				{u_12_15 = uint(bitfieldExtract(uint(u_6_7), int(0u), int(16u)))
				} * u_16_8))
			}, 
			{u_8_18 = uint(bitfieldExtract(uint(u_8_17), int(0u), int(16u)))
			}, int(16u), int(16u))
		} << 16u)
	} + 
	{u_11_11 = uint((
		{u_11_10 = uint(bitfieldExtract(uint(u_6_7), int(0u), int(16u)))
		} * 
		{u_12_14 = uint(bitfieldExtract(uint(u_8_17), int(0u), int(16u)))
		}))
	})
}
*/
	u_6_11 = u_6_10 + u_8_21;	// 0
	u_8_22 = uint(int(0 ) - int(u_9_21 ) );	// 0
	u_7_8 = u_8_22 + u_7_7;	/* 0  <=>  (uint((int(0) - int({u_9_21 : 0}))) + (({u_9_20 : 0} + uint((int(0) - int(((uint({u_12_12 : 0}) >= uint({u_7_3 : 1})) ? 4294967295u : 0u))))) ^ {u_9_21 : 0}))
<=>
(uint((int(0) - int(u_9_21))) + ((u_9_20 + uint((int(0) - int(((uint(u_12_12) >= uint(u_7_3)) ? 4294967295u : 0u))))) ^ u_9_21))
<=>
{u_7_8 = (
	{u_8_22 = uint((int(0) - int(u_9_21)))
	} + 
	{u_7_7 = (
		{u_7_6 = (u_9_20 + 
			{u_7_5 = uint((int(0) - int(
				{u_7_4 = (
					{b_3_12 = (uint(u_12_12) >= uint(u_7_3))
					} ? 4294967295u : 0u)
				})))
			})
		} ^ u_9_21)
	})
}
*/
	pf_3_10 = pf_8_8 * -0.5f;	// 100.595
	f_1_27 = 0.f - (pf_2_6 );	// -36.30958
	pf_2_7 = f_1_27 + f_10_8;	// 213.6904
	f_1_28 = utof(vs_cbuf9[78 ].w  );	// 1.00
	f_1_29 = (1.0f ) / f_1_28;	// 1.00
	f_8_16 = utof(vs_cbuf9[76 ].z  );	// 0
	f_9_19 = utof(vs_cbuf9[76 ].x  );	// 2.00
	pf_8_9 = f_9_19 + f_8_16;	// 2.00
	pf_3_11 = fma(0.f, pf_9_7, pf_3_10 );	// 100.595
	f_8_17 = utof(vs_cbuf9[75 ].x  );	// 1.00
	pf_15_10 = f_0_6 * f_8_17;	// 0.5484
	u_7_9 = b_1_25 ? (u_7_8) : (4294967295u);	// 0
	f_8_18 = float(int(u_7_9 ) );	// 0
	f_9_20 = utof(vs_cbuf9[76 ].z  );	// 0
	pf_8_10 = fma(f_0_6, f_9_20, pf_8_9 );	// 2.00
	pf_3_12 = fma(0.f, pf_11_3, pf_3_11 );	// 100.595
	pf_13_7 = fma(pf_15_10, -2.f, pf_13_6 );	// -0.09679997
	pf_6_4 = fma(0.f, pf_6_3, pf_14_20 );	// 1.00
	pf_5_7 = fma(0.f, pf_5_6, pf_16_9 );	// 0
	out_attr6.y  = pf_6_4;	/* 1.00  <=>  ((0.f * ((({pf_7_1 : 0} * {(vs_cbuf10_5.z) : 0}) + (({pf_6_1 : 0} * {(vs_cbuf10_5.y) : 1.00}) + ({pf_3_4 : 0} * {(vs_cbuf10_5.x) : 0}))) + {(vs_cbuf10_5.w) : 35.58958})) + (({utof(u_15_phi_27) : 0} * (0.f - {(vs_cbuf10_9.z) : 0})) + (({utof(u_12_phi_26) : 1.00} * {(vs_cbuf10_9.y) : 1.00}) + ({utof(u_11_phi_25) : 0} * {(vs_cbuf10_9.x) : 0}))))
<=>
((0.f * (((pf_7_1 * (vs_cbuf10_5.z)) + ((pf_6_1 * (vs_cbuf10_5.y)) + (pf_3_4 * (vs_cbuf10_5.x)))) + (vs_cbuf10_5.w))) + ((utof(u_15_phi_27) * (0.f - (vs_cbuf10_9.z))) + ((utof(u_12_phi_26) * (vs_cbuf10_9.y)) + (utof(u_11_phi_25) * (vs_cbuf10_9.x)))))
<=>
{out_attr6.y = 
	{pf_6_4 = ((0.f * 
		{pf_6_3 = (
			{pf_9_2 = ((pf_7_1 * 
				{f_2_18 = (vs_cbuf10_5.z)
				}) + 
				{pf_9_1 = ((pf_6_1 * 
					{f_2_11 = (vs_cbuf10_5.y)
					}) + 
					{pf_9_0 = (pf_3_4 * 
						{f_2_5 = (vs_cbuf10_5.x)
						})
					})
				})
			} + 
			{f_2_24 = (vs_cbuf10_5.w)
			})
		}) + 
		{pf_14_20 = ((
			{f_10_6 = utof(u_15_phi_27)
			} * 
			{f_9_16 = (0.f - 
				{f_9_15 = (vs_cbuf10_9.z)
				})
			}) + 
			{pf_14_19 = ((
				{f_9_11 = utof(u_12_phi_26)
				} * 
				{f_1_19 = (vs_cbuf10_9.y)
				}) + 
				{pf_19_0 = (
					{f_9_8 = utof(u_11_phi_25)
					} * 
					{f_1_14 = (vs_cbuf10_9.x)
					})
				})
			})
		})
	}
}
*/
	f_0_7 = utof(vs_cbuf9[78 ].y  );	// 1.00
	pf_6_5 = f_1_29 * f_0_7;	/* 1.00  <=>  ((1.0f / {utof(vs_cbuf9[78].w) : 1.00}) * {utof(vs_cbuf9[78].y) : 1.00})
<=>
((1.0f / utof(vs_cbuf9[78].w)) * utof(vs_cbuf9[78].y))
<=>
{pf_6_5 = (
	{f_1_29 = (1.0f / 
		{f_1_28 = utof(vs_cbuf9[78].w)
		})
	} * 
	{f_0_7 = utof(vs_cbuf9[78].y)
	})
}
*/
	out_attr6.x  = pf_5_7;	/* 0  <=>  ((0.f * ((({pf_7_1 : 0} * {(vs_cbuf10_4.z) : 0}) + (({pf_6_1 : 0} * {(vs_cbuf10_4.y) : 0}) + ({pf_3_4 : 0} * {(vs_cbuf10_4.x) : 1.00}))) + {(vs_cbuf10_4.w) : -1134.2701})) + (({utof(u_15_phi_27) : 0} * (0.f - {(vs_cbuf10_8.z) : 0})) + (({utof(u_12_phi_26) : 1.00} * {(vs_cbuf10_8.y) : 0}) + ({utof(u_11_phi_25) : 0} * {(vs_cbuf10_8.x) : 1.00}))))
<=>
((0.f * (((pf_7_1 * (vs_cbuf10_4.z)) + ((pf_6_1 * (vs_cbuf10_4.y)) + (pf_3_4 * (vs_cbuf10_4.x)))) + (vs_cbuf10_4.w))) + ((utof(u_15_phi_27) * (0.f - (vs_cbuf10_8.z))) + ((utof(u_12_phi_26) * (vs_cbuf10_8.y)) + (utof(u_11_phi_25) * (vs_cbuf10_8.x)))))
<=>
{out_attr6.x = 
	{pf_5_7 = ((0.f * 
		{pf_5_6 = (
			{pf_5_5 = ((pf_7_1 * 
				{f_2_12 = (vs_cbuf10_4.z)
				}) + 
				{pf_5_4 = ((pf_6_1 * 
					{f_2_7 = (vs_cbuf10_4.y)
					}) + 
					{pf_5_3 = (pf_3_4 * 
						{f_3_9 = (vs_cbuf10_4.x)
						})
					})
				})
			} + 
			{f_2_19 = (vs_cbuf10_4.w)
			})
		}) + 
		{pf_16_9 = ((
			{f_9_17 = utof(u_15_phi_27)
			} * 
			{f_8_14 = (0.f - 
				{f_8_13 = (vs_cbuf10_8.z)
				})
			}) + 
			{pf_19_1 = ((
				{f_9_13 = utof(u_12_phi_26)
				} * 
				{f_1_21 = (vs_cbuf10_8.y)
				}) + 
				{pf_20_0 = (
					{f_9_9 = utof(u_11_phi_25)
					} * 
					{f_1_15 = (vs_cbuf10_8.x)
					})
				})
			})
		})
	}
}
*/
	f_0_8 = utof(vs_cbuf9[74 ].y  );	// 0
	pf_1_16 = fma(pf_0_1, f_0_8, pf_1_15 );	// -0.92102003
	f_0_9 = utof(vs_cbuf9[77 ].x  );	// 0.0008727
	pf_5_8 = fma(pf_0_1, f_0_9, pf_7_6 );	// 0.8722283
	f_0_10 = utof(vs_cbuf9[75 ].w  );	// 0
	pf_7_7 = fma(pf_0_1, f_0_10, pf_12_14 );	// 2.00
	pf_3_13 = fma(pf_4_21, 0.5f, pf_3_12 );	// 345.5264
	f_0_11 = utof(vs_cbuf9[74 ].x  );	// 0
	f_0_12 = 0.f - (f_0_11 );	// 0
	f_1_30 = 0.f - (pf_13_7 );	// 0.0968
	pf_12_15 = fma(pf_0_1, f_0_12, f_1_30 );	// 0.0968
	out_attr3.y  = pf_3_13;	/* 345.5264  <=>  (({pf_4_21 : 489.8628} * 0.5f) + ((0.f * {pf_11_3 : 489.6667}) + ((0.f * (({pf_10_5 : 1.00} * {(view_proj[4].w) : 0}) + (({pf_9_6 : -489.8628} * {(view_proj[4].z) : 0}) + (({pf_8_6 : -93.81641} * {(view_proj[4].y) : 0}) + ({pf_4_17 : -719.3513} * {(view_proj[4].x) : 1.206285}))))) + ({pf_8_8 : -201.18993} * -0.5f))))
<=>
((pf_4_21 * 0.5f) + ((0.f * pf_11_3) + ((0.f * ((pf_10_5 * (view_proj[4].w)) + ((pf_9_6 * (view_proj[4].z)) + ((pf_8_6 * (view_proj[4].y)) + (pf_4_17 * (view_proj[4].x)))))) + (pf_8_8 * -0.5f))))
<=>
{out_attr3.y = 
	{pf_3_13 = ((pf_4_21 * 0.5f) + 
		{pf_3_12 = ((0.f * pf_11_3) + 
			{pf_3_11 = ((0.f * 
				{pf_9_7 = ((pf_10_5 * 
					{f_2_57 = (view_proj[4].w)
					}) + 
					{pf_11_2 = ((pf_9_6 * 
						{f_2_53 = (view_proj[4].z)
						}) + 
						{pf_12_1 = ((pf_8_6 * 
							{f_2_49 = (view_proj[4].y)
							}) + 
							{pf_12_0 = (pf_4_17 * 
								{f_2_44 = (view_proj[4].x)
								})
							})
						})
					})
				}) + 
				{pf_3_10 = (pf_8_8 * -0.5f)
				})
			})
		})
	}
}
*/
	f_0_13 = utof(vs_cbuf9[75 ].z  );	// 0
	pf_0_2 = fma(pf_0_1, f_0_13, pf_8_10 );	// 2.00
	f_0_14 = utof(vs_cbuf9[104 ].x  );	// 0.30
	pf_8_11 = pf_18_4 * f_0_14;	// 0.1418074
	f_0_15 = utof(u_14_phi_29 );	// 0.50196
	pf_13_8 = fma(pf_6_5, f_0_15, -0.5f );	// 0.00196
	out_attr1.y  = pf_8_11;	/* 0.1418074  <=>  (({utof(vs_cbuf9[121].y) : 0.4726913} * {(vs_cbuf10_1.y) : 1.00}) * {utof(vs_cbuf9[104].x) : 0.30})
<=>
((utof(vs_cbuf9[121].y) * (vs_cbuf10_1.y)) * utof(vs_cbuf9[104].x))
<=>
{out_attr1.y = 
	{pf_8_11 = (
		{pf_18_4 = (
			{f_11_7 = utof(vs_cbuf9[121].y)
			} * 
			{f_1_25 = (vs_cbuf10_1.y)
			})
		} * 
		{f_0_14 = utof(vs_cbuf9[104].x)
		})
	}
}
*/
	f_0_16 = 0.f - (f_8_18 );	// 0
	pf_1_17 = fma(pf_6_5, f_0_16, pf_1_16 );	// -0.92102003
	pf_3_14 = pf_3_13 * f_2_68;	// 0.7053533
	f_0_17 = utof(vs_cbuf9[78 ].z  );	// 1.00
	f_0_18 = (1.0f ) / f_0_17;	// 1.00
	f_1_31 = utof(vs_cbuf15[24 ].y  );	// -0.04761905
	f_2_69 = utof(vs_cbuf15[24 ].x  );	// 0.002381
	f_1_32 = 0.f - (f_1_31 );	// 0.0476191
	pf_6_6 = fma(pf_17_1, f_2_69, f_1_32 );	// 1.213658
	f_1_33 = min(max(pf_6_6, 0.0 ), 1.0 );	// 1.00
	pf_3_15 = fma(pf_3_14, -0.7f, 0.85f );	// 0.3562527
	f_2_70 = min(max(pf_3_15, 0.0 ), 1.0 );	// 0.3562527
	f_8_19 = utof(u_1_6 );
	f_9_21 = utof(u_0_8 );
	f_9_22 = 0.f - (f_9_21 );
	pf_3_16 = f_9_22 + f_8_19;	/* ((0.f - {utof(u_0_8) : }) + {utof(uvec4({vs_ssbo_color2.x * vs_ssbo_scale : }, {vs_ssbo_color2.y * vs_ssbo_scale : }, {vs_ssbo_color2.z * vs_ssbo_scale : }, {vs_ssbo_color2.w * vs_ssbo_scale : }).x) : })
<=>
((0.f - utof(u_0_8)) + utof(uvec4(vs_ssbo_color2.x * vs_ssbo_scale, vs_ssbo_color2.y * vs_ssbo_scale, vs_ssbo_color2.z * vs_ssbo_scale, vs_ssbo_color2.w * vs_ssbo_scale).x))
<=>
{pf_3_16 = (
	{f_9_22 = (0.f - 
		{f_9_21 = utof(u_0_8)
		})
	} + 
	{f_8_19 = utof(
		{u_1_6 = 
			{u4_0_0 = uvec4(vs_ssbo_color2.x * vs_ssbo_scale, vs_ssbo_color2.y * vs_ssbo_scale, vs_ssbo_color2.z * vs_ssbo_scale, vs_ssbo_color2.w * vs_ssbo_scale)
			}.x
		})
	})
}
*/
	f_8_20 = utof(u_2_8 );
	f_9_23 = utof(u_4_8 );
	f_9_24 = 0.f - (f_9_23 );
	pf_6_7 = f_9_24 + f_8_20;	/* ((0.f - {utof(u_4_8) : }) + {utof(uvec4({vs_ssbo_color2.x * vs_ssbo_scale : }, {vs_ssbo_color2.y * vs_ssbo_scale : }, {vs_ssbo_color2.z * vs_ssbo_scale : }, {vs_ssbo_color2.w * vs_ssbo_scale : }).y) : })
<=>
((0.f - utof(u_4_8)) + utof(uvec4(vs_ssbo_color2.x * vs_ssbo_scale, vs_ssbo_color2.y * vs_ssbo_scale, vs_ssbo_color2.z * vs_ssbo_scale, vs_ssbo_color2.w * vs_ssbo_scale).y))
<=>
{pf_6_7 = (
	{f_9_24 = (0.f - 
		{f_9_23 = utof(u_4_8)
		})
	} + 
	{f_8_20 = utof(
		{u_2_8 = 
			{u4_0_0 = uvec4(vs_ssbo_color2.x * vs_ssbo_scale, vs_ssbo_color2.y * vs_ssbo_scale, vs_ssbo_color2.z * vs_ssbo_scale, vs_ssbo_color2.w * vs_ssbo_scale)
			}.y
		})
	})
}
*/
	f_8_21 = utof(vs_cbuf15[27 ].x  );	// -0.14285715
	f_9_25 = utof(vs_cbuf15[27 ].y  );	// 0.0071429
	pf_2_8 = fma(pf_2_7, f_9_25, f_8_21 );	// 1.383512
	f_8_22 = min(max(pf_2_8, 0.0 ), 1.0 );	// 1.00
	f_1_34 = 0.f - (f_1_33 );	// -1
	pf_2_9 = f_1_34 + 1.f;	// 0
	f_1_35 = 0.f - (pf_5_8 );	/* -0.87222826  <=>  (0.f - (({pf_0_1 : 999.50} * {utof(vs_cbuf9[77].x) : 0.0008727}) + (({in_attr7.z : 0.58835} * {utof(vs_cbuf9[77].z) : 0}) + ({utof(vs_cbuf9[77].z) : 0} + {utof(vs_cbuf9[77].y) : 0}))))
<=>
(0.f - ((pf_0_1 * utof(vs_cbuf9[77].x)) + ((in_attr7.z * utof(vs_cbuf9[77].z)) + (utof(vs_cbuf9[77].z) + utof(vs_cbuf9[77].y)))))
<=>
{f_1_35 = (0.f - 
	{pf_5_8 = ((pf_0_1 * 
		{f_0_9 = utof(vs_cbuf9[77].x)
		}) + 
		{pf_7_6 = ((
			{f_13_11 = in_attr7.z
			} * 
			{f_9_3 = utof(vs_cbuf9[77].z)
			}) + 
			{pf_7_5 = (
				{f_17_1 = utof(vs_cbuf9[77].z)
				} + 
				{f_14_5 = utof(vs_cbuf9[77].y)
				})
			})
		})
	})
}
*/
	f_9_26 = log2(pf_2_9 );	// -∞
	f_10_9 = 0.f - (f_2_70 );	/* -0.3562527  <=>  (0.f - clamp(((((({pf_4_21 : 489.8628} * 0.5f) + ((0.f * {pf_11_3 : 489.6667}) + ((0.f * (({pf_10_5 : 1.00} * {(view_proj[4].w) : 0}) + (({pf_9_6 : -489.8628} * {(view_proj[4].z) : 0}) + (({pf_8_6 : -93.81641} * {(view_proj[4].y) : 0}) + ({pf_4_17 : -719.3513} * {(view_proj[4].x) : 1.206285}))))) + ({pf_8_8 : -201.18993} * -0.5f)))) * (1.0f / ({pf_4_21 : 489.8628} + ((0.f * {pf_11_3 : 489.6667}) + {pf_13_2 : -0})))) * -0.7f) + 0.85f), 0.0, 1.0))
<=>
(0.f - clamp((((((pf_4_21 * 0.5f) + ((0.f * pf_11_3) + ((0.f * ((pf_10_5 * (view_proj[4].w)) + ((pf_9_6 * (view_proj[4].z)) + ((pf_8_6 * (view_proj[4].y)) + (pf_4_17 * (view_proj[4].x)))))) + (pf_8_8 * -0.5f)))) * (1.0f / (pf_4_21 + ((0.f * pf_11_3) + pf_13_2)))) * -0.7f) + 0.85f), 0.0, 1.0))
<=>
{f_10_9 = (0.f - 
	{f_2_70 = clamp(
		{pf_3_15 = ((
			{pf_3_14 = (
				{pf_3_13 = ((pf_4_21 * 0.5f) + 
					{pf_3_12 = ((0.f * pf_11_3) + 
						{pf_3_11 = ((0.f * 
							{pf_9_7 = ((pf_10_5 * 
								{f_2_57 = (view_proj[4].w)
								}) + 
								{pf_11_2 = ((pf_9_6 * 
									{f_2_53 = (view_proj[4].z)
									}) + 
									{pf_12_1 = ((pf_8_6 * 
										{f_2_49 = (view_proj[4].y)
										}) + 
										{pf_12_0 = (pf_4_17 * 
											{f_2_44 = (view_proj[4].x)
											})
										})
									})
								})
							}) + 
							{pf_3_10 = (pf_8_8 * -0.5f)
							})
						})
					})
				} * 
				{f_2_68 = (1.0f / 
					{pf_15_1 = (pf_4_21 + 
						{pf_15_0 = ((0.f * pf_11_3) + pf_13_2)
						})
					})
				})
			} * -0.7f) + 0.85f)
		}, 0.0, 1.0)
	})
}
*/
	pf_2_10 = fma(pf_3_16, f_10_9, pf_3_16 );
	f_10_10 = cos(f_1_35 );	// 0.6431218
	f_11_9 = 0.f - (f_2_70 );	/* -0.3562527  <=>  (0.f - clamp(((((({pf_4_21 : 489.8628} * 0.5f) + ((0.f * {pf_11_3 : 489.6667}) + ((0.f * (({pf_10_5 : 1.00} * {(view_proj[4].w) : 0}) + (({pf_9_6 : -489.8628} * {(view_proj[4].z) : 0}) + (({pf_8_6 : -93.81641} * {(view_proj[4].y) : 0}) + ({pf_4_17 : -719.3513} * {(view_proj[4].x) : 1.206285}))))) + ({pf_8_8 : -201.18993} * -0.5f)))) * (1.0f / ({pf_4_21 : 489.8628} + ((0.f * {pf_11_3 : 489.6667}) + {pf_13_2 : -0})))) * -0.7f) + 0.85f), 0.0, 1.0))
<=>
(0.f - clamp((((((pf_4_21 * 0.5f) + ((0.f * pf_11_3) + ((0.f * ((pf_10_5 * (view_proj[4].w)) + ((pf_9_6 * (view_proj[4].z)) + ((pf_8_6 * (view_proj[4].y)) + (pf_4_17 * (view_proj[4].x)))))) + (pf_8_8 * -0.5f)))) * (1.0f / (pf_4_21 + ((0.f * pf_11_3) + pf_13_2)))) * -0.7f) + 0.85f), 0.0, 1.0))
<=>
{f_11_9 = (0.f - 
	{f_2_70 = clamp(
		{pf_3_15 = ((
			{pf_3_14 = (
				{pf_3_13 = ((pf_4_21 * 0.5f) + 
					{pf_3_12 = ((0.f * pf_11_3) + 
						{pf_3_11 = ((0.f * 
							{pf_9_7 = ((pf_10_5 * 
								{f_2_57 = (view_proj[4].w)
								}) + 
								{pf_11_2 = ((pf_9_6 * 
									{f_2_53 = (view_proj[4].z)
									}) + 
									{pf_12_1 = ((pf_8_6 * 
										{f_2_49 = (view_proj[4].y)
										}) + 
										{pf_12_0 = (pf_4_17 * 
											{f_2_44 = (view_proj[4].x)
											})
										})
									})
								})
							}) + 
							{pf_3_10 = (pf_8_8 * -0.5f)
							})
						})
					})
				} * 
				{f_2_68 = (1.0f / 
					{pf_15_1 = (pf_4_21 + 
						{pf_15_0 = ((0.f * pf_11_3) + pf_13_2)
						})
					})
				})
			} * -0.7f) + 0.85f)
		}, 0.0, 1.0)
	})
}
*/
	pf_3_17 = fma(pf_6_7, f_11_9, pf_6_7 );
	f_1_36 = sin(f_1_35 );	// -0.7657639
	f_11_10 = utof(vs_cbuf15[28 ].y  );	// -0.4663191
	pf_5_9 = pf_17_1 + f_11_10;	// 489.2603
	u_1_7 = b_0_9 ? (u_6_11) : (4294967295u);	/* 0  <=>  (({b_1_22 : False} || {b_0_8 : True}) ? ((uint((uint(bitfieldExtract(uint({u_6_7 : 1}), int(16u), int(16u))) * {u_12_17 : 0})) << 16u) + {u_8_21 : 0}) : 4294967295u)
<=>
((b_1_22 || b_0_8) ? ((uint((uint(bitfieldExtract(uint(u_6_7), int(16u), int(16u))) * u_12_17)) << 16u) + u_8_21) : 4294967295u)
<=>
{u_1_7 = (
	{b_0_9 = (b_1_22 || b_0_8)
	} ? 
	{u_6_11 = (
		{u_6_10 = (
			{u_6_9 = uint((
				{u_6_8 = uint(bitfieldExtract(uint(u_6_7), int(16u), int(16u)))
				} * u_12_17))
			} << 16u)
		} + u_8_21)
	} : 4294967295u)
}
*/
	f_11_11 = utof(u_3_8 );
	f_12_4 = utof(u_5_8 );
	f_12_5 = 0.f - (f_12_4 );
	pf_6_8 = f_12_5 + f_11_11;	/* ((0.f - {utof(u_5_8) : }) + {utof(uvec4({vs_ssbo_color2.x * vs_ssbo_scale : }, {vs_ssbo_color2.y * vs_ssbo_scale : }, {vs_ssbo_color2.z * vs_ssbo_scale : }, {vs_ssbo_color2.w * vs_ssbo_scale : }).z) : })
<=>
((0.f - utof(u_5_8)) + utof(uvec4(vs_ssbo_color2.x * vs_ssbo_scale, vs_ssbo_color2.y * vs_ssbo_scale, vs_ssbo_color2.z * vs_ssbo_scale, vs_ssbo_color2.w * vs_ssbo_scale).z))
<=>
{pf_6_8 = (
	{f_12_5 = (0.f - 
		{f_12_4 = utof(u_5_8)
		})
	} + 
	{f_11_11 = utof(
		{u_3_8 = 
			{u4_0_0 = uvec4(vs_ssbo_color2.x * vs_ssbo_scale, vs_ssbo_color2.y * vs_ssbo_scale, vs_ssbo_color2.z * vs_ssbo_scale, vs_ssbo_color2.w * vs_ssbo_scale)
			}.z
		})
	})
}
*/
	f_11_12 = float(int(u_1_7 ) );	// 0
	f_12_6 = utof(vs_cbuf9[78 ].x  );	// 1.00
	pf_8_12 = f_0_18 * f_12_6;	/* 1.00  <=>  ((1.0f / {utof(vs_cbuf9[78].z) : 1.00}) * {utof(vs_cbuf9[78].x) : 1.00})
<=>
((1.0f / utof(vs_cbuf9[78].z)) * utof(vs_cbuf9[78].x))
<=>
{pf_8_12 = (
	{f_0_18 = (1.0f / 
		{f_0_17 = utof(vs_cbuf9[78].z)
		})
	} * 
	{f_12_6 = utof(vs_cbuf9[78].x)
	})
}
*/
	pf_5_10 = pf_5_9 * f_15_1;	// 611.1771
	f_0_19 = utof(u_0_8 );
	pf_2_11 = f_0_19 + pf_2_10;	/* ({utof(u_0_8) : } + (({pf_3_16 : } * {f_10_9 : -0.3562527}) + {pf_3_16 : }))
<=>
(utof(u_0_8) + ((pf_3_16 * f_10_9) + pf_3_16))
<=>
{pf_2_11 = (
	{f_0_19 = utof(u_0_8)
	} + 
	{pf_2_10 = ((pf_3_16 * f_10_9) + pf_3_16)
	})
}
*/
	f_0_20 = utof(vs_cbuf15[58 ].w  );	// 1.00
	f_12_7 = utof(vs_cbuf15[58 ].x  );	// 1.00
	f_13_13 = utof(vs_cbuf15[58 ].w  );	// 1.00
	f_0_21 = 0.f - (f_0_20 );	// -1
	pf_14_21 = fma(f_13_13, f_12_7, f_0_21 );	// 0
	f_0_22 = utof(vs_cbuf15[23 ].x  );	// 20.00
	pf_15_11 = f_3_24 * f_0_22;	// -0.5232732
	f_0_23 = utof(vs_cbuf15[24 ].w  );	// 4.00
	pf_16_10 = f_9_26 * f_0_23;	// -∞
	f_0_24 = 0.f - (f_2_70 );	/* -0.3562527  <=>  (0.f - clamp(((((({pf_4_21 : 489.8628} * 0.5f) + ((0.f * {pf_11_3 : 489.6667}) + ((0.f * (({pf_10_5 : 1.00} * {(view_proj[4].w) : 0}) + (({pf_9_6 : -489.8628} * {(view_proj[4].z) : 0}) + (({pf_8_6 : -93.81641} * {(view_proj[4].y) : 0}) + ({pf_4_17 : -719.3513} * {(view_proj[4].x) : 1.206285}))))) + ({pf_8_8 : -201.18993} * -0.5f)))) * (1.0f / ({pf_4_21 : 489.8628} + ((0.f * {pf_11_3 : 489.6667}) + {pf_13_2 : -0})))) * -0.7f) + 0.85f), 0.0, 1.0))
<=>
(0.f - clamp((((((pf_4_21 * 0.5f) + ((0.f * pf_11_3) + ((0.f * ((pf_10_5 * (view_proj[4].w)) + ((pf_9_6 * (view_proj[4].z)) + ((pf_8_6 * (view_proj[4].y)) + (pf_4_17 * (view_proj[4].x)))))) + (pf_8_8 * -0.5f)))) * (1.0f / (pf_4_21 + ((0.f * pf_11_3) + pf_13_2)))) * -0.7f) + 0.85f), 0.0, 1.0))
<=>
{f_0_24 = (0.f - 
	{f_2_70 = clamp(
		{pf_3_15 = ((
			{pf_3_14 = (
				{pf_3_13 = ((pf_4_21 * 0.5f) + 
					{pf_3_12 = ((0.f * pf_11_3) + 
						{pf_3_11 = ((0.f * 
							{pf_9_7 = ((pf_10_5 * 
								{f_2_57 = (view_proj[4].w)
								}) + 
								{pf_11_2 = ((pf_9_6 * 
									{f_2_53 = (view_proj[4].z)
									}) + 
									{pf_12_1 = ((pf_8_6 * 
										{f_2_49 = (view_proj[4].y)
										}) + 
										{pf_12_0 = (pf_4_17 * 
											{f_2_44 = (view_proj[4].x)
											})
										})
									})
								})
							}) + 
							{pf_3_10 = (pf_8_8 * -0.5f)
							})
						})
					})
				} * 
				{f_2_68 = (1.0f / 
					{pf_15_1 = (pf_4_21 + 
						{pf_15_0 = ((0.f * pf_11_3) + pf_13_2)
						})
					})
				})
			} * -0.7f) + 0.85f)
		}, 0.0, 1.0)
	})
}
*/
	pf_6_9 = fma(pf_6_8, f_0_24, pf_6_8 );
	pf_18_5 = f_10_10 * pf_13_8;	// 0.0012605
	f_0_25 = utof(u_13_phi_30 );	// 0.50196
	pf_20_2 = fma(pf_8_12, f_0_25, -0.5f );	// 0.00196
	pf_13_9 = pf_13_8 * f_1_36;	// -0.0015008815
	pf_5_11 = pf_5_10 * 0.06666667f;	// 40.74514
	pf_2_12 = fma(pf_2_11, pf_14_21, pf_2_11 );	/* (({pf_2_11 : } * (({(vs_cbuf15_58.w) : 1.00} * {(vs_cbuf15_58.x) : 1.00}) + (0.f - {(vs_cbuf15_58.w) : 1.00}))) + {pf_2_11 : })
<=>
((pf_2_11 * (((vs_cbuf15_58.w) * (vs_cbuf15_58.x)) + (0.f - (vs_cbuf15_58.w)))) + pf_2_11)
<=>
{pf_2_12 = ((pf_2_11 * 
	{pf_14_21 = ((
		{f_13_13 = (vs_cbuf15_58.w)
		} * 
		{f_12_7 = (vs_cbuf15_58.x)
		}) + 
		{f_0_21 = (0.f - 
			{f_0_20 = (vs_cbuf15_58.w)
			})
		})
	}) + pf_2_11)
}
*/
	f_0_26 = utof(u_4_8 );
	pf_3_18 = f_0_26 + pf_3_17;	/* ({utof(u_4_8) : } + (({pf_6_7 : } * {f_11_9 : -0.3562527}) + {pf_6_7 : }))
<=>
(utof(u_4_8) + ((pf_6_7 * f_11_9) + pf_6_7))
<=>
{pf_3_18 = (
	{f_0_26 = utof(u_4_8)
	} + 
	{pf_3_17 = ((pf_6_7 * f_11_9) + pf_6_7)
	})
}
*/
	out_attr9.x  = pf_2_12;	/* {pf_2_12 : }
<=>
pf_2_12
<=>
{out_attr9.x = pf_2_12
}
*/
	f_0_27 = utof(u_5_8 );
	pf_2_13 = f_0_27 + pf_6_9;	/* ({utof(u_5_8) : } + (({pf_6_8 : } * {f_0_24 : -0.3562527}) + {pf_6_8 : }))
<=>
(utof(u_5_8) + ((pf_6_8 * f_0_24) + pf_6_8))
<=>
{pf_2_13 = (
	{f_0_27 = utof(u_5_8)
	} + 
	{pf_6_9 = ((pf_6_8 * f_0_24) + pf_6_8)
	})
}
*/
	f_0_28 = exp2(pf_15_11 );	/* 0.6957914  <=>  exp2((log2(((0.f - clamp((({pf_17_1 : 489.7266} * {(vs_cbuf15_22.x) : 0.0000333}) + (0.f - {(vs_cbuf15_22.y) : -0.0016638935})), 0.0, 1.0)) + 1.f)) * {(vs_cbuf15_23.x) : 20.00}))
<=>
exp2((log2(((0.f - clamp(((pf_17_1 * (vs_cbuf15_22.x)) + (0.f - (vs_cbuf15_22.y))), 0.0, 1.0)) + 1.f)) * (vs_cbuf15_23.x)))
<=>
{f_0_28 = exp2(
	{pf_15_11 = (
		{f_3_24 = log2(
			{pf_14_6 = (
				{f_3_23 = (0.f - 
					{f_3_22 = clamp(
						{pf_14_5 = ((pf_17_1 * 
							{f_4_5 = (vs_cbuf15_22.x)
							}) + 
							{f_3_21 = (0.f - 
								{f_3_20 = (vs_cbuf15_22.y)
								})
							})
						}, 0.0, 1.0)
					})
				} + 1.f)
			})
		} * 
		{f_0_22 = (vs_cbuf15_23.x)
		})
	})
}
*/
	f_2_71 = utof(vs_cbuf15[58 ].w  );	// 1.00
	f_3_25 = utof(vs_cbuf15[58 ].y  );	// 1.00
	f_9_27 = utof(vs_cbuf15[58 ].w  );	// 1.00
	f_2_72 = 0.f - (f_2_71 );	// -1
	pf_6_10 = fma(f_9_27, f_3_25, f_2_72 );	// 0
	f_2_73 = exp2(pf_16_10 );	/* 0  <=>  exp2((log2(((0.f - clamp((({pf_17_1 : 489.7266} * {(vs_cbuf15_24.x) : 0.002381}) + (0.f - {(vs_cbuf15_24.y) : -0.04761905})), 0.0, 1.0)) + 1.f)) * {(vs_cbuf15_24.w) : 4.00}))
<=>
exp2((log2(((0.f - clamp(((pf_17_1 * (vs_cbuf15_24.x)) + (0.f - (vs_cbuf15_24.y))), 0.0, 1.0)) + 1.f)) * (vs_cbuf15_24.w)))
<=>
{f_2_73 = exp2(
	{pf_16_10 = (
		{f_9_26 = log2(
			{pf_2_9 = (
				{f_1_34 = (0.f - 
					{f_1_33 = clamp(
						{pf_6_6 = ((pf_17_1 * 
							{f_2_69 = (vs_cbuf15_24.x)
							}) + 
							{f_1_32 = (0.f - 
								{f_1_31 = (vs_cbuf15_24.y)
								})
							})
						}, 0.0, 1.0)
					})
				} + 1.f)
			})
		} * 
		{f_0_23 = (vs_cbuf15_24.w)
		})
	})
}
*/
	f_3_26 = utof(vs_cbuf15[58 ].w  );	// 1.00
	f_9_28 = utof(vs_cbuf15[58 ].z  );	// 1.00
	f_12_8 = utof(vs_cbuf15[58 ].w  );	// 1.00
	f_3_27 = 0.f - (f_3_26 );	// -1
	pf_14_22 = fma(f_12_8, f_9_28, f_3_27 );	// 0
	pf_15_12 = fma(pf_20_2, f_1_36, pf_18_5 );	// -0.00024037588
	f_1_37 = 0.f - (pf_13_9 );	// 0.0015009
	pf_13_10 = fma(f_10_10, pf_20_2, f_1_37 );	// 0.0027614
	pf_8_13 = fma(pf_8_12, f_11_12, pf_12_15 );	// 0.0968
	pf_9_8 = fma(pf_9_7, 0.5f, pf_10_6 );	// -433.87134
	f_1_38 = max(pf_5_11, 0.2f );	// 40.74514
	pf_3_19 = fma(pf_3_18, pf_6_10, pf_3_18 );	/* (({pf_3_18 : } * (({(vs_cbuf15_58.w) : 1.00} * {(vs_cbuf15_58.y) : 1.00}) + (0.f - {(vs_cbuf15_58.w) : 1.00}))) + {pf_3_18 : })
<=>
((pf_3_18 * (((vs_cbuf15_58.w) * (vs_cbuf15_58.y)) + (0.f - (vs_cbuf15_58.w)))) + pf_3_18)
<=>
{pf_3_19 = ((pf_3_18 * 
	{pf_6_10 = ((
		{f_9_27 = (vs_cbuf15_58.w)
		} * 
		{f_3_25 = (vs_cbuf15_58.y)
		}) + 
		{f_2_72 = (0.f - 
			{f_2_71 = (vs_cbuf15_58.w)
			})
		})
	}) + pf_3_18)
}
*/
	f_3_28 = utof(vs_cbuf15[49 ].x  );	// 0
	b_0_10 = 0.f != f_3_28 || isnan(0.f ) || isnan(f_3_28 );	// False
	out_attr9.y  = pf_3_19;	/* {pf_3_19 : }
<=>
pf_3_19
<=>
{out_attr9.y = pf_3_19
}
*/
	pf_2_14 = fma(pf_2_13, pf_14_22, pf_2_13 );	/* (({pf_2_13 : } * (({(vs_cbuf15_58.w) : 1.00} * {(vs_cbuf15_58.z) : 1.00}) + (0.f - {(vs_cbuf15_58.w) : 1.00}))) + {pf_2_13 : })
<=>
((pf_2_13 * (((vs_cbuf15_58.w) * (vs_cbuf15_58.z)) + (0.f - (vs_cbuf15_58.w)))) + pf_2_13)
<=>
{pf_2_14 = ((pf_2_13 * 
	{pf_14_22 = ((
		{f_12_8 = (vs_cbuf15_58.w)
		} * 
		{f_9_28 = (vs_cbuf15_58.z)
		}) + 
		{f_3_27 = (0.f - 
			{f_3_26 = (vs_cbuf15_58.w)
			})
		})
	}) + pf_2_13)
}
*/
	pf_0_3 = fma(pf_0_2, pf_13_10, pf_8_13 );	// 0.1023227
	out_attr9.z  = pf_2_14;	/* {pf_2_14 : }
<=>
pf_2_14
<=>
{out_attr9.z = pf_2_14
}
*/
	f_3_29 = 0.f - (pf_1_17 );	// 0.92102
	pf_1_18 = fma(pf_15_12, pf_7_7, f_3_29 );	// 0.9205393
	pf_2_15 = fma(0.f, pf_11_3, pf_9_8 );	// -433.87134
	f_1_39 = min(max(f_1_38, 0.0 ), 1.0 );	// 1.00
	f_3_30 = utof(vs_cbuf9[104 ].x  );	// 0.30
	pf_3_20 = pf_19_2 * f_3_30;	// 0.1452381
	f_3_31 = utof(vs_cbuf15[26 ].w  );	// 0.20
	pf_5_12 = f_8_22 * f_3_31;	// 0.20
	out_attr1.z  = pf_3_20;	/* 0.1452381  <=>  (({utof(vs_cbuf9[121].z) : 0.484127} * {(vs_cbuf10_1.z) : 1.00}) * {utof(vs_cbuf9[104].x) : 0.30})
<=>
((utof(vs_cbuf9[121].z) * (vs_cbuf10_1.z)) * utof(vs_cbuf9[104].x))
<=>
{out_attr1.z = 
	{pf_3_20 = (
		{pf_19_2 = (
			{f_11_8 = utof(vs_cbuf9[121].z)
			} * 
			{f_1_26 = (vs_cbuf10_1.z)
			})
		} * 
		{f_3_30 = utof(vs_cbuf9[104].x)
		})
	}
}
*/
	pf_0_4 = pf_0_3 + 0.5f;	// 0.6023228
	out_attr8.y  = pf_5_12;	/* 0.20  <=>  (clamp(((((0.f - {pf_2_6 : 36.30958}) + min({(camera_wpos.y) : 365.7373}, {(vs_cbuf15_27.z) : 250.00})) * {(vs_cbuf15_27.y) : 0.0071429}) + {(vs_cbuf15_27.x) : -0.14285715}), 0.0, 1.0) * {(vs_cbuf15_26.w) : 0.20})
<=>
(clamp(((((0.f - pf_2_6) + min((camera_wpos.y), (vs_cbuf15_27.z))) * (vs_cbuf15_27.y)) + (vs_cbuf15_27.x)), 0.0, 1.0) * (vs_cbuf15_26.w))
<=>
{out_attr8.y = 
	{pf_5_12 = (
		{f_8_22 = clamp(
			{pf_2_8 = ((
				{pf_2_7 = (
					{f_1_27 = (0.f - pf_2_6)
					} + 
					{f_10_8 = min(
						{f_11_6 = (camera_wpos.y)
						}, 
						{f_10_7 = (vs_cbuf15_27.z)
						})
					})
				} * 
				{f_9_25 = (vs_cbuf15_27.y)
				}) + 
				{f_8_21 = (vs_cbuf15_27.x)
				})
			}, 0.0, 1.0)
		} * 
		{f_3_31 = (vs_cbuf15_26.w)
		})
	}
}
*/
	pf_2_16 = fma(pf_4_21, 0.5f, pf_2_15 );	// -188.93994
	out_attr2.x  = pf_0_4;	/* 0.6023228  <=>  ((((({pf_0_1 : 999.50} * {utof(vs_cbuf9[75].z) : 0}) + (({in_attr7.x : 0.5484} * {utof(vs_cbuf9[76].z) : 0}) + ({utof(vs_cbuf9[76].x) : 2.00} + {utof(vs_cbuf9[76].z) : 0}))) * ((cos({f_1_35 : -0.87222826}) * (({pf_8_12 : 1.00} * {utof(u_13_phi_30) : 0.50196}) + -0.5f)) + (0.f - ((({pf_6_5 : 1.00} * {utof(u_14_phi_29) : 0.50196}) + -0.5f) * sin({f_1_35 : -0.87222826}))))) + (({pf_8_12 : 1.00} * float(int({u_1_7 : 0}))) + (({pf_0_1 : 999.50} * (0.f - {utof(vs_cbuf9[74].x) : 0})) + (0.f - ((({in_attr7.x : 0.5484} * {utof(vs_cbuf9[75].x) : 1.00}) * -2.f) + ({utof(vs_cbuf9[75].x) : 1.00} + {utof(vs_cbuf9[74].z) : 0})))))) + 0.5f)
<=>
(((((pf_0_1 * utof(vs_cbuf9[75].z)) + ((in_attr7.x * utof(vs_cbuf9[76].z)) + (utof(vs_cbuf9[76].x) + utof(vs_cbuf9[76].z)))) * ((cos(f_1_35) * ((pf_8_12 * utof(u_13_phi_30)) + -0.5f)) + (0.f - (((pf_6_5 * utof(u_14_phi_29)) + -0.5f) * sin(f_1_35))))) + ((pf_8_12 * float(int(u_1_7))) + ((pf_0_1 * (0.f - utof(vs_cbuf9[74].x))) + (0.f - (((in_attr7.x * utof(vs_cbuf9[75].x)) * -2.f) + (utof(vs_cbuf9[75].x) + utof(vs_cbuf9[74].z))))))) + 0.5f)
<=>
{out_attr2.x = 
	{pf_0_4 = (
		{pf_0_3 = ((
			{pf_0_2 = ((pf_0_1 * 
				{f_0_13 = utof(vs_cbuf9[75].z)
				}) + 
				{pf_8_10 = ((
					{f_0_6 = in_attr7.x
					} * 
					{f_9_20 = utof(vs_cbuf9[76].z)
					}) + 
					{pf_8_9 = (
						{f_9_19 = utof(vs_cbuf9[76].x)
						} + 
						{f_8_16 = utof(vs_cbuf9[76].z)
						})
					})
				})
			} * 
			{pf_13_10 = ((
				{f_10_10 = cos(f_1_35)
				} * 
				{pf_20_2 = ((pf_8_12 * 
					{f_0_25 = utof(u_13_phi_30)
					}) + -0.5f)
				}) + 
				{f_1_37 = (0.f - 
					{pf_13_9 = (
						{pf_13_8 = ((pf_6_5 * 
							{f_0_15 = utof(u_14_phi_29)
							}) + -0.5f)
						} * 
						{f_1_36 = sin(f_1_35)
						})
					})
				})
			}) + 
			{pf_8_13 = ((pf_8_12 * 
				{f_11_12 = float(int(u_1_7))
				}) + 
				{pf_12_15 = ((pf_0_1 * 
					{f_0_12 = (0.f - 
						{f_0_11 = utof(vs_cbuf9[74].x)
						})
					}) + 
					{f_1_30 = (0.f - 
						{pf_13_7 = ((
							{pf_15_10 = (f_0_6 * 
								{f_8_17 = utof(vs_cbuf9[75].x)
								})
							} * -2.f) + 
							{pf_13_6 = (
								{f_19_1 = utof(vs_cbuf9[75].x)
								} + 
								{f_11_4 = utof(vs_cbuf9[74].z)
								})
							})
						})
					})
				})
			})
		} + 0.5f)
	}
}
*/
	out_attr3.x  = pf_2_16;	/* -188.93994  <=>  (({pf_4_21 : 489.8628} * 0.5f) + ((0.f * {pf_11_3 : 489.6667}) + (((({pf_10_5 : 1.00} * {(view_proj[4].w) : 0}) + (({pf_9_6 : -489.8628} * {(view_proj[4].z) : 0}) + (({pf_8_6 : -93.81641} * {(view_proj[4].y) : 0}) + ({pf_4_17 : -719.3513} * {(view_proj[4].x) : 1.206285})))) * 0.5f) + (0.f * {pf_8_8 : -201.18993}))))
<=>
((pf_4_21 * 0.5f) + ((0.f * pf_11_3) + ((((pf_10_5 * (view_proj[4].w)) + ((pf_9_6 * (view_proj[4].z)) + ((pf_8_6 * (view_proj[4].y)) + (pf_4_17 * (view_proj[4].x))))) * 0.5f) + (0.f * pf_8_8))))
<=>
{out_attr3.x = 
	{pf_2_16 = ((pf_4_21 * 0.5f) + 
		{pf_2_15 = ((0.f * pf_11_3) + 
			{pf_9_8 = ((
				{pf_9_7 = ((pf_10_5 * 
					{f_2_57 = (view_proj[4].w)
					}) + 
					{pf_11_2 = ((pf_9_6 * 
						{f_2_53 = (view_proj[4].z)
						}) + 
						{pf_12_1 = ((pf_8_6 * 
							{f_2_49 = (view_proj[4].y)
							}) + 
							{pf_12_0 = (pf_4_17 * 
								{f_2_44 = (view_proj[4].x)
								})
							})
						})
					})
				} * 0.5f) + 
				{pf_10_6 = (0.f * pf_8_8)
				})
			})
		})
	}
}
*/
	pf_0_5 = pf_1_18 + 0.5f;	// 1.420539
	f_3_32 = utof(vs_cbuf10[3 ].x  );	// 1.00
	out_attr4.x  = f_3_32;	/* 1.00  <=>  {(vs_cbuf10_3.x) : 1.00}
<=>
(vs_cbuf10_3.x)
<=>
{out_attr4.x = 
	{f_3_32 = (vs_cbuf10_3.x)
	}
}
*/
	f_3_33 = utof(vs_cbuf15[23 ].z  );	// 0.85
	f_8_23 = utof(vs_cbuf15[23 ].z  );	// 0.85
	f_8_24 = 0.f - (f_8_23 );	// -0.85
	pf_1_19 = fma(f_0_28, f_8_24, f_3_33 );	// 0.2585773
	f_0_29 = min(max(pf_1_19, 0.0 ), 1.0 );	// 0.2585773
	out_attr2.y  = pf_0_5;	/* 1.420539  <=>  ((((((({pf_8_12 : 1.00} * {utof(u_13_phi_30) : 0.50196}) + -0.5f) * sin({f_1_35 : -0.87222826})) + (cos({f_1_35 : -0.87222826}) * (({pf_6_5 : 1.00} * {utof(u_14_phi_29) : 0.50196}) + -0.5f))) * (({pf_0_1 : 999.50} * {utof(vs_cbuf9[75].w) : 0}) + (({in_attr7.y : 0.96051} * {utof(vs_cbuf9[76].w) : 0}) + ({utof(vs_cbuf9[76].y) : 2.00} + {utof(vs_cbuf9[76].w) : 0})))) + (0.f - (({pf_6_5 : 1.00} * (0.f - float(int((({b_2_11 : False} || {b_1_24 : True}) ? {u_7_8 : 0} : 4294967295u))))) + (({pf_0_1 : 999.50} * {utof(vs_cbuf9[74].y) : 0}) + ((({in_attr7.y : 0.96051} * {utof(vs_cbuf9[75].y) : 1.00}) * -2.f) + ({utof(vs_cbuf9[75].y) : 1.00} + {utof(vs_cbuf9[74].w) : 0})))))) + 0.5f)
<=>
(((((((pf_8_12 * utof(u_13_phi_30)) + -0.5f) * sin(f_1_35)) + (cos(f_1_35) * ((pf_6_5 * utof(u_14_phi_29)) + -0.5f))) * ((pf_0_1 * utof(vs_cbuf9[75].w)) + ((in_attr7.y * utof(vs_cbuf9[76].w)) + (utof(vs_cbuf9[76].y) + utof(vs_cbuf9[76].w))))) + (0.f - ((pf_6_5 * (0.f - float(int(((b_2_11 || b_1_24) ? u_7_8 : 4294967295u))))) + ((pf_0_1 * utof(vs_cbuf9[74].y)) + (((in_attr7.y * utof(vs_cbuf9[75].y)) * -2.f) + (utof(vs_cbuf9[75].y) + utof(vs_cbuf9[74].w))))))) + 0.5f)
<=>
{out_attr2.y = 
	{pf_0_5 = (
		{pf_1_18 = ((
			{pf_15_12 = ((
				{pf_20_2 = ((pf_8_12 * 
					{f_0_25 = utof(u_13_phi_30)
					}) + -0.5f)
				} * 
				{f_1_36 = sin(f_1_35)
				}) + 
				{pf_18_5 = (
					{f_10_10 = cos(f_1_35)
					} * 
					{pf_13_8 = ((pf_6_5 * 
						{f_0_15 = utof(u_14_phi_29)
						}) + -0.5f)
					})
				})
			} * 
			{pf_7_7 = ((pf_0_1 * 
				{f_0_10 = utof(vs_cbuf9[75].w)
				}) + 
				{pf_12_14 = ((
					{f_11_3 = in_attr7.y
					} * 
					{f_19_0 = utof(vs_cbuf9[76].w)
					}) + 
					{pf_12_13 = (
						{f_17_2 = utof(vs_cbuf9[76].y)
						} + 
						{f_16_3 = utof(vs_cbuf9[76].w)
						})
					})
				})
			}) + 
			{f_3_29 = (0.f - 
				{pf_1_17 = ((pf_6_5 * 
					{f_0_16 = (0.f - 
						{f_8_18 = float(int(
							{u_7_9 = (
								{b_1_25 = (b_2_11 || b_1_24)
								} ? u_7_8 : 4294967295u)
							}))
						})
					}) + 
					{pf_1_16 = ((pf_0_1 * 
						{f_0_8 = utof(vs_cbuf9[74].y)
						}) + 
						{pf_1_15 = ((
							{pf_1_14 = (f_11_3 * 
								{f_18_0 = utof(vs_cbuf9[75].y)
								})
							} * -2.f) + 
							{pf_12_12 = (
								{f_16_2 = utof(vs_cbuf9[75].y)
								} + 
								{f_8_2 = utof(vs_cbuf9[74].w)
								})
							})
						})
					})
				})
			})
		} + 0.5f)
	}
}
*/
	f_3_34 = utof(vs_cbuf15[25 ].w  );	// 0.7006614
	f_8_25 = utof(vs_cbuf15[25 ].w  );	// 0.7006614
	f_8_26 = 0.f - (f_8_25 );	// -0.7006614
	pf_0_6 = fma(f_2_73, f_8_26, f_3_34 );	// 0.7006614
	out_attr10.w  = f_0_29;	/* 0.2585773  <=>  clamp((({f_0_28 : 0.6957914} * (0.f - {(vs_cbuf15_23.z) : 0.85})) + {(vs_cbuf15_23.z) : 0.85}), 0.0, 1.0)
<=>
clamp(((f_0_28 * (0.f - (vs_cbuf15_23.z))) + (vs_cbuf15_23.z)), 0.0, 1.0)
<=>
{out_attr10.w = 
	{f_0_29 = clamp(
		{pf_1_19 = ((f_0_28 * 
			{f_8_24 = (0.f - 
				{f_8_23 = (vs_cbuf15_23.z)
				})
			}) + 
			{f_3_33 = (vs_cbuf15_23.z)
			})
		}, 0.0, 1.0)
	}
}
*/
	pf_1_20 = f_1_39 * f_4_10;	// 0.50
	out_attr8.x  = pf_0_6;	/* 0.7006614  <=>  (({f_2_73 : 0} * (0.f - {(vs_cbuf15_25.w) : 0.7006614})) + {(vs_cbuf15_25.w) : 0.7006614})
<=>
((f_2_73 * (0.f - (vs_cbuf15_25.w))) + (vs_cbuf15_25.w))
<=>
{out_attr8.x = 
	{pf_0_6 = ((f_2_73 * 
		{f_8_26 = (0.f - 
			{f_8_25 = (vs_cbuf15_25.w)
			})
		}) + 
		{f_3_34 = (vs_cbuf15_25.w)
		})
	}
}
*/
	pf_0_7 = f_1_39 * f_5_4;	// 0.50
	out_attr10.x  = pf_1_20;	/* 0.50  <=>  (clamp(max(((({pf_17_1 : 489.7266} + {(lightDir.y) : -0.4663191}) * (1.0f / clamp((({(lightDir.y) : -0.4663191} * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0) * {f4_0_0.x : 0.50})
<=>
(clamp(max((((pf_17_1 + (lightDir.y)) * (1.0f / clamp((((lightDir.y) * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0) * f4_0_0.x)
<=>
{out_attr10.x = 
	{pf_1_20 = (
		{f_1_39 = clamp(
			{f_1_38 = max(
				{pf_5_11 = (
					{pf_5_10 = (
						{pf_5_9 = (pf_17_1 + 
							{f_11_10 = (lightDir.y)
							})
						} * 
						{f_15_1 = (1.0f / 
							{f_16_1 = clamp(
								{pf_1_13 = ((
									{f_16_0 = (lightDir.y)
									} * 1.5f) + 1.5f)
								}, 0.0, 1.0)
							})
						})
					} * 0.06666667f)
				}, 0.2f)
			}, 0.0, 1.0)
		} * 
		{f_4_10 = f4_0_0.x
		})
	}
}
*/
	pf_1_21 = f_1_39 * f_6_1;	// 0.50
	out_attr10.y  = pf_0_7;	/* 0.50  <=>  (clamp(max(((({pf_17_1 : 489.7266} + {(lightDir.y) : -0.4663191}) * (1.0f / clamp((({(lightDir.y) : -0.4663191} * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0) * {f4_0_0.y : 0.50})
<=>
(clamp(max((((pf_17_1 + (lightDir.y)) * (1.0f / clamp((((lightDir.y) * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0) * f4_0_0.y)
<=>
{out_attr10.y = 
	{pf_0_7 = (
		{f_1_39 = clamp(
			{f_1_38 = max(
				{pf_5_11 = (
					{pf_5_10 = (
						{pf_5_9 = (pf_17_1 + 
							{f_11_10 = (lightDir.y)
							})
						} * 
						{f_15_1 = (1.0f / 
							{f_16_1 = clamp(
								{pf_1_13 = ((
									{f_16_0 = (lightDir.y)
									} * 1.5f) + 1.5f)
								}, 0.0, 1.0)
							})
						})
					} * 0.06666667f)
				}, 0.2f)
			}, 0.0, 1.0)
		} * 
		{f_5_4 = f4_0_0.y
		})
	}
}
*/
	out_attr10.z  = pf_1_21;	/* 0.50  <=>  (clamp(max(((({pf_17_1 : 489.7266} + {(lightDir.y) : -0.4663191}) * (1.0f / clamp((({(lightDir.y) : -0.4663191} * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0) * {f4_0_0.z : 0.50})
<=>
(clamp(max((((pf_17_1 + (lightDir.y)) * (1.0f / clamp((((lightDir.y) * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0) * f4_0_0.z)
<=>
{out_attr10.z = 
	{pf_1_21 = (
		{f_1_39 = clamp(
			{f_1_38 = max(
				{pf_5_11 = (
					{pf_5_10 = (
						{pf_5_9 = (pf_17_1 + 
							{f_11_10 = (lightDir.y)
							})
						} * 
						{f_15_1 = (1.0f / 
							{f_16_1 = clamp(
								{pf_1_13 = ((
									{f_16_0 = (lightDir.y)
									} * 1.5f) + 1.5f)
								}, 0.0, 1.0)
							})
						})
					} * 0.06666667f)
				}, 0.2f)
			}, 0.0, 1.0)
		} * 
		{f_6_1 = f4_0_0.z
		})
	}
}
*/
	b_0_11 = ! b_0_10;	// True
	b_1_26 = b_0_11 ? (true) : (false);	// True
	if(b_1_26 )	/* True  <=>  if(((! (((0.f != {(vs_cbuf15_49.x) : 0}) || isnan(0.f)) || isnan({(vs_cbuf15_49.x) : 0}))) ? true : false))
<=>
if(((! (((0.f != (vs_cbuf15_49.x)) || isnan(0.f)) || isnan((vs_cbuf15_49.x)))) ? true : false))
<=>if(b_1_26...)
*/
	{
		return;
	} 
	f_0_30 = utof(vs_cbuf15[51 ].x  );	// 950.00
	f_0_31 = (1.0f ) / f_0_30;	/* 0.0010526  <=>  (1.0f / {(vs_cbuf15_51.x) : 950.00})
<=>
(1.0f / (vs_cbuf15_51.x))
<=>
{f_0_31 = (1.0f / 
	{f_0_30 = (vs_cbuf15_51.x)
	})
}
*/
	f_1_40 = utof(vs_cbuf15[49 ].x  );	// 0
	f_2_74 = utof(vs_cbuf15[49 ].x  );	// 0
	f_1_41 = 0.f - (f_1_40 );	// 0
	pf_0_8 = fma(f_7_24, f_2_74, f_1_41 );	// 0
	pf_0_9 = pf_0_8 + 1.f;	/* 1.00  <=>  ((({f4_0_1.w : 1.00} * {(vs_cbuf15_49.x) : 0}) + (0.f - {(vs_cbuf15_49.x) : 0})) + 1.f)
<=>
(((f4_0_1.w * (vs_cbuf15_49.x)) + (0.f - (vs_cbuf15_49.x))) + 1.f)
<=>
{pf_0_9 = (
	{pf_0_8 = ((
		{f_7_24 = f4_0_1.w
		} * 
		{f_2_74 = (vs_cbuf15_49.x)
		}) + 
		{f_1_41 = (0.f - 
			{f_1_40 = (vs_cbuf15_49.x)
			})
		})
	} + 1.f)
}
*/
	f_1_42 = utof(vs_cbuf15[51 ].y  );	// 50.00
	pf_1_22 = f_0_31 * f_1_42;	// 0.0526316
	f_1_43 = 0.f - (pf_1_22 );	// -0.052631576
	pf_1_23 = fma(pf_17_1, f_0_31, f_1_43 );	// 0.4628701
	f_0_32 = min(max(pf_1_23, 0.0 ), 1.0 );	/* 0.4628701  <=>  clamp((({pf_17_1 : 489.7266} * {f_0_31 : 0.0010526}) + (0.f - ({f_0_31 : 0.0010526} * {(vs_cbuf15_51.y) : 50.00}))), 0.0, 1.0)
<=>
clamp(((pf_17_1 * f_0_31) + (0.f - (f_0_31 * (vs_cbuf15_51.y)))), 0.0, 1.0)
<=>
{f_0_32 = clamp(
	{pf_1_23 = ((pf_17_1 * f_0_31) + 
		{f_1_43 = (0.f - 
			{pf_1_22 = (f_0_31 * 
				{f_1_42 = (vs_cbuf15_51.y)
				})
			})
		})
	}, 0.0, 1.0)
}
*/
	f_1_44 = 0.f - (f_0_32 );	// -0.4628701
	pf_1_24 = fma(pf_0_9, f_1_44, f_0_32 );	// 0
	f_0_33 = abs(pf_1_24 );	// 0
	f_0_34 = log2(f_0_33 );	/* -∞  <=>  log2(abs((({pf_0_9 : 1.00} * (0.f - {f_0_32 : 0.4628701})) + {f_0_32 : 0.4628701})))
<=>
log2(abs(((pf_0_9 * (0.f - f_0_32)) + f_0_32)))
<=>
{f_0_34 = log2(
	{f_0_33 = abs(
		{pf_1_24 = ((pf_0_9 * 
			{f_1_44 = (0.f - f_0_32)
			}) + f_0_32)
		})
	})
}
*/
	f_1_45 = utof(vs_cbuf15[51 ].z  );	// 1.50
	pf_1_25 = f_0_34 * f_1_45;	// -∞
	f_0_35 = exp2(pf_1_25 );	/* 0  <=>  exp2(({f_0_34 : -∞} * {(vs_cbuf15_51.z) : 1.50}))
<=>
exp2((f_0_34 * (vs_cbuf15_51.z)))
<=>
{f_0_35 = exp2(
	{pf_1_25 = (f_0_34 * 
		{f_1_45 = (vs_cbuf15_51.z)
		})
	})
}
*/
	f_1_46 = utof(vs_cbuf15[51 ].w  );	// 1.00
	pf_1_26 = f_0_35 * f_1_46;	// 0
	f_0_36 = utof(vs_cbuf15[49 ].x  );	// 0
	pf_1_27 = pf_1_26 * f_0_36;	// 0
	f_0_37 = 0.f - (pf_1_27 );	// 0
	pf_0_10 = fma(pf_0_9, f_0_37, pf_0_9 );	// 1.00
	out_attr7.x  = pf_0_10;	/* 1.00  <=>  (({pf_0_9 : 1.00} * (0.f - (({f_0_35 : 0} * {(vs_cbuf15_51.w) : 1.00}) * {(vs_cbuf15_49.x) : 0}))) + {pf_0_9 : 1.00})
<=>
((pf_0_9 * (0.f - ((f_0_35 * (vs_cbuf15_51.w)) * (vs_cbuf15_49.x)))) + pf_0_9)
<=>
{out_attr7.x = 
	{pf_0_10 = ((pf_0_9 * 
		{f_0_37 = (0.f - 
			{pf_1_27 = (
				{pf_1_26 = (f_0_35 * 
					{f_1_46 = (vs_cbuf15_51.w)
					})
				} * 
				{f_0_36 = (vs_cbuf15_49.x)
				})
			})
		}) + pf_0_9)
	}
}
*/
	return;
} 
/*split_variable_assignment{
	b_0_8,
	b_1_22,
	b_1_24,
	b_2_11,
	f_0_24,
	f_0_28,
	f_0_31,
	f_0_32,
	f_0_34,
	f_0_35,
	f_1_35,
	f_10_9,
	f_11_9,
	f_13_6,
	f_13_7,
	f_14_4,
	f_16_6,
	f_2_73,
	f_3_15,
	f_5_3,
	f_7_20,
	f_8_6,
	f4_0_0,
	f4_0_1,
	pf_0_1,
	pf_0_9,
	pf_1_1,
	pf_1_10,
	pf_1_6,
	pf_10_5,
	pf_11_3,
	pf_12_3,
	pf_12_8,
	pf_12_9,
	pf_13_2,
	pf_16_0,
	pf_17_0,
	pf_17_1,
	pf_2_11,
	pf_2_12,
	pf_2_13,
	pf_2_14,
	pf_2_4,
	pf_2_6,
	pf_3_16,
	pf_3_18,
	pf_3_19,
	pf_3_4,
	pf_4_13,
	pf_4_17,
	pf_4_21,
	pf_4_3,
	pf_6_1,
	pf_6_5,
	pf_6_7,
	pf_6_8,
	pf_7_1,
	pf_7_2,
	pf_8_12,
	pf_8_6,
	pf_8_8,
	pf_9_6,
	u_0_1,
	u_0_2,
	u_0_3,
	u_0_4,
	u_0_7,
	u_0_8,
	u_0_phi_19,
	u_0_phi_21,
	u_1_1,
	u_1_2,
	u_1_5,
	u_1_7,
	u_1_phi_18,
	u_11_2,
	u_11_3,
	u_11_6,
	u_11_8,
	u_11_9,
	u_11_phi_25,
	u_12_1,
	u_12_11,
	u_12_12,
	u_12_17,
	u_12_3,
	u_12_4,
	u_12_phi_26,
	u_13_10,
	u_13_6,
	u_13_7,
	u_13_9,
	u_13_phi_30,
	u_14_6,
	u_14_7,
	u_14_phi_29,
	u_15_3,
	u_15_5,
	u_15_8,
	u_15_9,
	u_15_phi_27,
	u_16_0,
	u_16_8,
	u_17_0,
	u_19_4,
	u_2_1,
	u_2_2,
	u_2_4,
	u_2_5,
	u_2_phi_11,
	u_2_phi_4,
	u_20_1,
	u_20_2,
	u_20_3,
	u_20_4,
	u_20_phi_23,
	u_20_phi_24,
	u_3_3,
	u_3_4,
	u_3_6,
	u_3_7,
	u_3_phi_16,
	u_3_phi_20,
	u_4_0,
	u_4_1,
	u_4_2,
	u_4_3,
	u_4_5,
	u_4_6,
	u_4_8,
	u_4_phi_2,
	u_4_phi_20,
	u_4_phi_9,
	u_5_4,
	u_5_5,
	u_5_6,
	u_5_7,
	u_5_8,
	u_5_phi_15,
	u_5_phi_17,
	u_6_4,
	u_6_5,
	u_6_7,
	u_6_phi_20,
	u_7_3,
	u_7_8,
	u_8_10,
	u_8_15,
	u_8_17,
	u_8_21,
	u_9_14,
	u_9_20,
	u_9_21,
	u_9_7,
	u_9_9,
}*/
