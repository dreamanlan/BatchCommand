skill("flip-image")
{
    tool {
        document("call_skill('flip-image', image_path, flip_direction); flip_direction: 0=horizontal, 1=vertical");
        command($image_path, $flip_direction)
        {:
            python {% basepath %}/skills/flip-image/flip-image.py {% $image_path %} {% $flip_direction %}
        :};
    };
};
