﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/CoulombEquipotentialLineShader" {
    Properties {
        _MainTex ("Sprite Texture", 2D) = "white" {}        
        _LineColor("Line Color", Color) = (1,1,1,1)
        _BkgdColor("Background Color", Color) = (1,1,1,1)
        _LineWidth("Line Width", Range(0, 1)) = 0.0001
        _ChargeStepTolerance("Charge Step Tolerance", Range(0.0,1.0)) = 0.001
        _MaxDistance("Maxium Distance", Range(0.0, 100.0)) = 22.0
        _DistanceStep("Distance Steps", Range(0.0, 100.0)) = 1.0
    }
    
    SubShader {
//        Tags {"Queue"="Background"  "IgnoreProjector"="True"}
        Tags {"Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100
        
        ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass {
        CGPROGRAM
        #pragma vertex vert  
        #pragma fragment frag
        #include "UnityCG.cginc"
        
        float _ChargeStepTolerance;
        float _MaxDistance;
        float _DistanceStep;
        float4 _LineColor;
        float4 _BkgdColor;
        float _LineWidth;
        
        uniform int _EntryCnt;
        uniform float4 _Entries[100];       
        
        struct vertInput {
            float3 pos : POSITION;
        };  
        
        struct vertOutput {
            float4 pos : SV_POSITION;
            float4 cmp_pos : TEXCOORD0;
        };
        
        vertOutput vert(vertInput input) {
            vertOutput o;
            
            o.cmp_pos = float4(input.pos, 1);
            o.pos = UnityObjectToClipPos(input.pos);
            return o;
        }
        
        half4 frag(vertOutput input) : COLOR {
            float CoulombConstant = 9; 
            float CoulombMultiplyFactor = 0.001;
            float3 pos, pos1, pos2, pos3, pos4;
            pos = pos1 = pos2 = pos3 = pos4 = input.cmp_pos.xyz;
            pos1.x = pos2.x = input.cmp_pos.x - _LineWidth;
            pos3.x = pos4.x = input.cmp_pos.x + _LineWidth;
            
            pos1.z = pos3.z = input.cmp_pos.z - _LineWidth;
            pos2.z = pos4.z = input.cmp_pos.z + _LineWidth;
            
            float3 world_pos = mul (unity_ObjectToWorld, pos);
            float3 world_pos1 = mul (unity_ObjectToWorld, pos1);
            float3 world_pos2 = mul (unity_ObjectToWorld, pos2);
            float3 world_pos3 = mul (unity_ObjectToWorld, pos3);
            float3 world_pos4 = mul (unity_ObjectToWorld, pos4);
                        
            //TODO: check voltage on 4 neigbouring points -> not a tolerance for the voltage -> use linewidth
             int entries = _EntryCnt;
            float voltage = 0.0;
            float voltage1 = 0.0;
            float voltage2 = 0.0;
            float voltage3 = 0.0;
            float voltage4 = 0.0;
            
            float radius = 0.71;
            for(int i = 0; i < entries; ++i){
                float3 posEntry = _Entries[i].xyz;
                float charge = _Entries[i].w;
                
                float dist = distance(world_pos, posEntry) - radius;
                if(dist < 0) dist = 0;
                float tmp = CoulombConstant * CoulombMultiplyFactor * charge / (dist * dist); //TODO: check formula
                voltage = voltage + tmp;
                
                dist = distance(world_pos1, posEntry) - radius;
                if(dist < 0) dist = 0;
                tmp = CoulombConstant * CoulombMultiplyFactor * charge / (dist * dist); //TODO: check formula
                voltage1 = voltage1 + tmp;
                
                
                dist = distance(world_pos2, posEntry) - radius;
                if(dist < 0) dist = 0;
                tmp = CoulombConstant * CoulombMultiplyFactor * charge / (dist * dist); //TODO: check formula
                voltage2 = voltage2 + tmp;
                
                
                dist = distance(world_pos3, posEntry) - radius;
                if(dist < 0) dist = 0;
                tmp = CoulombConstant * CoulombMultiplyFactor * charge / (dist * dist); //TODO: check formula
                voltage3 = voltage3 + tmp;
                
                
                dist = distance(world_pos4, posEntry) - radius;
                if(dist < 0) dist = 0;
                tmp = CoulombConstant * CoulombMultiplyFactor * charge / (dist * dist); //TODO: check formula
                voltage4 = voltage4 + tmp;
            }           
                  
             
            if(voltage < 0.0) voltage *= -1.0;
            if(voltage1 < 0.0) voltage1 *= -1.0;
            if(voltage2 < 0.0) voltage2 *= -1.0;
            if(voltage3 < 0.0) voltage3 *= -1.0;
            if(voltage4 < 0.0) voltage4 *= -1.0;
            
            if(_DistanceStep < 0.0001)
                return _BkgdColor;
            
            for(float distance = _DistanceStep / 2; distance <= _MaxDistance; distance += _DistanceStep){
                float VoltAtDistance = CoulombConstant * CoulombMultiplyFactor / (distance * distance);
                float UpperVoltAtDistance = CoulombConstant * CoulombMultiplyFactor / ((distance - _LineWidth)*(distance - _LineWidth));
                float LowerVoltAtDistance = CoulombConstant * CoulombMultiplyFactor / ((distance + _LineWidth)*(distance + _LineWidth));
                
                if(((voltage < LowerVoltAtDistance || voltage1 < LowerVoltAtDistance || voltage2 < LowerVoltAtDistance || voltage3 < LowerVoltAtDistance || voltage4 < LowerVoltAtDistance)
                && (voltage > UpperVoltAtDistance || voltage1 > UpperVoltAtDistance || voltage2 > UpperVoltAtDistance || voltage3 > UpperVoltAtDistance || voltage4 > UpperVoltAtDistance))
                || (LowerVoltAtDistance - _ChargeStepTolerance < voltage && voltage < UpperVoltAtDistance + _ChargeStepTolerance)
                ||(LowerVoltAtDistance - _ChargeStepTolerance < voltage1 && voltage1 < UpperVoltAtDistance + _ChargeStepTolerance)
                ||(LowerVoltAtDistance - _ChargeStepTolerance < voltage2 && voltage2 < UpperVoltAtDistance + _ChargeStepTolerance)
                ||(LowerVoltAtDistance - _ChargeStepTolerance < voltage3 && voltage3 < UpperVoltAtDistance + _ChargeStepTolerance)
                ||(LowerVoltAtDistance - _ChargeStepTolerance < voltage4 && voltage4 < UpperVoltAtDistance + _ChargeStepTolerance) ){
                
                    return _LineColor;
                }
            }
            
            return _BkgdColor;
        }

        ENDCG
        }
    }
}