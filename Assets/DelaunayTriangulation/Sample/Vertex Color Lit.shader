Shader "Vertex Color Lit" {
     Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
     }
     SubShader {
        Pass {
            Lighting Off
            ColorMaterial AmbientAndDiffuse
            SetTexture [_MainTex] {
               combine texture * primary DOUBLE
            }
        }
     }
 }