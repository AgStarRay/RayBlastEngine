#version 130

// Input vertex attributes
in vec2 fragTexCoord;
in vec4 fragColor;

// Input uniform values
uniform sampler2D texture0;
uniform vec4 colDiffuse;
uniform vec4 colOutline;
uniform float dilation = 0.5;
uniform float outlineDilation = 0.5;

// Output fragment color
out vec4 finalColor;

void main()
{
    // Calculate alpha using Signed Distance Field
    float textureAlpha = texture(texture0, fragTexCoord).a;
    float faceDistanceFromOutline = textureAlpha - 1.0 + dilation;
    float faceDistanceChangePerFragment = length(vec2(dFdx(faceDistanceFromOutline), dFdy(faceDistanceFromOutline)));
    float faceAlpha = smoothstep(-faceDistanceChangePerFragment, faceDistanceChangePerFragment, faceDistanceFromOutline);
    float outlineDistanceFromOutline = textureAlpha - 1.0 + outlineDilation;
    float outlineDistanceChangePerFragment = length(vec2(dFdx(outlineDistanceFromOutline), dFdy(outlineDistanceFromOutline)));
    float outlineAlpha = smoothstep(-outlineDistanceChangePerFragment, outlineDistanceChangePerFragment, outlineDistanceFromOutline);

    // Calculate fragment color
    vec4 outlineColor = vec4(colOutline.rgb, fragColor.a * outlineAlpha);
    finalColor = mix(outlineColor, fragColor, faceAlpha);
}
