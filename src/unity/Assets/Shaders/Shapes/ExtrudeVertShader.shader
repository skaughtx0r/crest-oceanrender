﻿// This file is subject to the MIT License as seen in the root of this folder structure (LICENSE)

Shader "Custom/Geometry/Extrude Vertex"
{
	Properties
	{
		_FactorParallel("FactorParallel", Range(0., 8.)) = 0.2
		_FactorOrthogonal("FactorOrthogonal", Range(0., 4.)) = 0.2
		_Strength("Strength", Range(0., 64.)) = 0.2
		_Velocity("Velocity", Vector) = (0,0,0,0)
	}

	SubShader
	{
		Tags{ "Queue" = "Transparent" }
		Blend One One

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				fixed4 col : COLOR;
				float offsetDist : TEXCOORD0;
			};

			float _FactorParallel, _FactorOrthogonal;
			float4 _Velocity;

			v2f vert(appdata_base v)
			{
				v2f o;

				float3 vertexWorldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

				o.normal = normalize(mul(unity_ObjectToWorld, float4(v.normal, 0.)).xyz);

				float3 vel =_Velocity /= 30.;

				float velMag = max(length(vel), 0.001);
				float3 velN = vel / velMag;
				float angleFactor = dot(velN, o.normal);

				if (angleFactor < 0.)
				{
					// this helps for when velocity exactly perpendicular to some faces
					if (angleFactor < -0.0001)
					{
						vel = -vel;
					}

					angleFactor *= -1.;
				}

				float3 offset = o.normal * _FactorOrthogonal * pow(1. - angleFactor, .2) * velMag;
				offset += vel * _FactorParallel * pow(angleFactor, .5);
				o.offsetDist = length(offset);
				vertexWorldPos += offset;

				o.vertex = mul(UNITY_MATRIX_VP, float4(vertexWorldPos, 1.));

				o.col = (fixed4)1.;

				return o;
			}

			float _Strength;

			fixed3 frag(v2f i) : SV_Target
			{
				fixed4 col = (fixed4)0.;
				col.x = _Strength * length(i.offsetDist) * abs(i.normal.y) * length(_Velocity) / 10.;

				if (dot(i.normal, _Velocity) < -0.1)
				{
					col.x *= -1.;
				}

				return fixed4(col.x/3600., col.x / 3600., 0., 0.);
			}
			ENDCG
		}
	}
}
