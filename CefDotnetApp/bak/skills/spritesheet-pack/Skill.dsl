skill("spritesheet-pack")
{
    tool {
        document("call_skill('spritesheet-pack', src, dst, rows, cols, frame_w, frame_h[, order]); split a grid pose sheet into frames, remove background to transparent, resize to frame_w x frame_h, concat horizontally into a single-row sprite sheet. order is optional comma-separated indices (row-major by default)");
        command($src, $dst, $rows, $cols, $frame_w, $frame_h, $order)
        {:
            python {% basepath %}/skills/spritesheet-pack/spritesheet_pack.py {% $src %} {% $dst %} {% $rows %} {% $cols %} {% $frame_w %} {% $frame_h %} {% $order %}
        :};
    };
};
