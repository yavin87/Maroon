﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Maroon.UI;

public class ParameterUI : MonoBehaviour, IResetObject
{
    private static ParameterUI _instance;
    private DialogueManager _dialogueManager;

    Dictionary<string, string> functions_ = new Dictionary<string, string>();
   
    public Dropdown dropdown;
    List<Dropdown.OptionData> menuOptions;

    [SerializeField] InputField fxIF;
    [SerializeField] InputField fyIF;
    [SerializeField] InputField fzIF;

    private Vector3 to_deltat_steps = new Vector3(0, 0, 0);
    private Vector3 xyz = new Vector3(0, 0, 0);
    private Vector3 vxvyvz = new Vector3(0, 0, 0);

    private float mass_ = 1f;
    private float t0_ = 0f;
    private float deltat_ = 0.05f;
    private float steps_ = 500f;

    private float x_ = 0f;
    private float y_ = 0f;
    private float z_ = 0f;

    private float vx_ = 1f;
    private float vy_ = 0f;
    private float vz_ = 0f;

    [SerializeField] InputField if_mass;

    [SerializeField] InputField if_t0;
    [SerializeField] InputField if_deltat;
    [SerializeField] InputField if_steps;

    [SerializeField] InputField if_x;
    [SerializeField] InputField if_y;
    [SerializeField] InputField if_z;

    [SerializeField] InputField if_vx;
    [SerializeField] InputField if_vy;
    [SerializeField] InputField if_vz;

    [SerializeField] private UnityEngine.UI.Toggle showLabelToggle_;
    [SerializeField] private UnityEngine.UI.Toggle showOriginGrid_;

    // Start is called before the first frame update
    void Start()
    {
        if (_dialogueManager == null)
            _dialogueManager = FindObjectOfType<DialogueManager>();

        functions_.Add("abs", "Abs");
        functions_.Add("acos", "Acos");
        functions_.Add("asin", "Asin");
        functions_.Add("atan", "Atan");
        functions_.Add("ceiling", "Ceiling");
        functions_.Add("cos", "Cos");
        functions_.Add("exp", "Exp");
        functions_.Add("floor", "Floor");
        functions_.Add("ieeeremainder", "IEEERemainder");
        functions_.Add("log", "Log");
        functions_.Add("log10", "Log10");
        functions_.Add("max", "Max");
        functions_.Add("min", "Min");
        functions_.Add("pow", "Pow");
        functions_.Add("round", "Round");
        functions_.Add("sign", "Sign");
        functions_.Add("sin", "Sin");
        functions_.Add("sqrt", "Sqrt");
        functions_.Add("tan", "Tan");
        functions_.Add("truncate", "Truncate");

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public string getFunctionFx()
    {
        return getCorrectedFormula(fxIF.text);
    }

    public string getFunctionFy()
    {
        return getCorrectedFormula(fyIF.text);
    }

    public string getFunctionFz()
    {
        return getCorrectedFormula(fzIF.text);
    }

    public float getMass()
    {
        if (mass_ <= 0)
        {
            mass_ = 1f;
            if_mass.text = "1";
        }
        
        return mass_;
    }

    public void setMass(string value)
    {
        mass_ = float.Parse(value); 
    }

    public void setT0(string value)
    {
        t0_ = System.Convert.ToSingle(value);
    }

    public void setDeltaT(string value)
    {
        deltat_ = float.Parse(value);
    }

    public void setSteps(string value)
    {
        steps_ = System.Convert.ToSingle(value);
    }

    public Vector3 getTimes()
    {
        if (t0_ < 0)
        {
            t0_ = 0f;
            if_t0.text = "0";
        }
        if (deltat_ <= 0)
        {
            deltat_ = 1f;
            if_deltat.text = "1";
        }     
        if (steps_ <= 0)
        {
            steps_ = 1f;
            if_steps.text = "1";
        }        

        to_deltat_steps = new Vector3(t0_, deltat_, steps_);

        return to_deltat_steps;
    }

    public void setX(string value)
    {
        x_ = System.Convert.ToSingle(value);
    }

    public void setY(string value)
    {
        y_ = System.Convert.ToSingle(value);
    }

    public void setZ(string value)
    {
        z_ = System.Convert.ToSingle(value);
    }

    public Vector3 getXYZ()
    {
        xyz = new Vector3(x_, y_, z_);

        return xyz;
    }

    public void setVX(string value)
    {
        vx_ = System.Convert.ToSingle(value);
    }

    public void setVY(string value)
    {
        vy_ = System.Convert.ToSingle(value);
    }

    public void setVZ(string value)
    {
        vz_ = System.Convert.ToSingle(value);
    }

    public Vector3 getVxVyVz()
    {
        vxvyvz = new Vector3(vx_, vy_, vz_);

        return vxvyvz;
    }

    public void loadParameters(int choice)
    {
        // liste aus objekten -> list[choice]
        SimulationController.Instance.ResetSimulation();

        switch (choice)
        {
            case 0:
                loadDefault();
                dropdown.SetValueWithoutNotify(0);
                break;
            case 1:
                loadSatellite();
                dropdown.SetValueWithoutNotify(1);
                break;
            case 2:
                loadBallInTheWind();
                dropdown.SetValueWithoutNotify(2);
                break;
            default:
                break;
        }
    }

    private void loadDefault()
    {
        fxIF.text = "-x";
        fyIF.text = "0";
        fzIF.text = "0";

        if_mass.text = "1";
        
        if_t0.text = "0";        
        if_deltat.text = "0,05";        
        if_steps.text = "500";
        
        if_x.text = "0";        
        if_y.text = "0";       
        if_z.text = "0";       

        if_vx.text = "1";       
        if_vy.text = "0";        
        if_vz.text = "0";
        
        mass_ = 1;
        t0_ = 0;
        deltat_ = 0.05f;
        steps_ = 500;
        x_ = 0;
        y_ = 0;
        z_ = 0;
        vx_ = 1;
        vy_ = 0;
        vz_ = 0;
    }

    private void loadSatellite()
    {
        fxIF.text = "-100*x*6.6726E-11*5.97219E24/Pow(x*x+y*y+z*z,3/2)";
        fyIF.text = "-100*y*6.6726E-11*5.97219E24/Pow(x*x+y*y+z*z,3/2)";
        fzIF.text = "-100*z*6.6726E-11*5.97219E24/Pow(x*x+y*y+z*z,3/2)";

        if_mass.text = "100";

        if_t0.text = "0";
        if_deltat.text = "60";
        if_steps.text = "1500";

        if_x.text = "0";
        if_y.text = "6371000";
        if_z.text = "0";

        if_vx.text = "7900";
        if_vy.text = "0";
        if_vz.text = "0";

        mass_ = 100;
        t0_ = 0;
        deltat_ = 60;
        steps_ = 1500;
        x_ = 0;
        y_ = 6371000;
        z_ = 0;
        vx_ = 7900;
        vy_ = 0;
        vz_ = 0;
    }

    private void loadBallInTheWind()
    {
        fxIF.text = "(-0.01*(vx-(1))-0.03*(vx-(1))*Sqrt((vx-(1))*(vx-(1))+(vy-(7*Exp(-x*x)))*(vy-(7*Exp(-x*x)))+(vz-(-3*Exp(-t*t)))*(vz-(-3*Exp(-t*t)))))";
        fyIF.text = "(-0.01*(vy-(7*Exp(-x*x)))-0.03*(vy-(7*Exp(-x*x)))*Sqrt((vx-(1))*(vx-(1))+(vy-(7*Exp(-x*x)))*(vy-(7*Exp(-x*x)))+(vz-(-3*Exp(-t*t)))*(vz-(-3*Exp(-t*t)))))";
        fzIF.text = "(-0.01*(vz-(-3*Exp(-t*t)))-0.03*(vz-(-3*Exp(-t*t)))*Sqrt((vx-(1))*(vx-(1))+(vy-(7*Exp(-x*x)))*(vy-(7*Exp(-x*x)))+(vz-(-3*Exp(-t*t)))*(vz-(-3*Exp(-t*t)))))-9.81*0.1";

        if_mass.text = "0,1";

        if_t0.text = "0";
        if_deltat.text = "0,01";
        if_steps.text = "100";

        if_x.text = "0";
        if_y.text = "0";
        if_z.text = "0";

        if_vx.text = "-7";
        if_vy.text = "5";
        if_vz.text = "10";

        mass_ = 0.1f;
        t0_ = 0;
        deltat_ = 0.01f;
        steps_ = 100;
        x_ = 0;
        y_ = 0;
        z_ = 0;
        vx_ = -7;
        vy_ = 5;
        vz_ = 10;
    }

    private string getCorrectedFormula(string formula)
    {
        string tmp = formula.ToLower();
        
        foreach (KeyValuePair<string, string> entry in functions_)
        {
            tmp = tmp.Replace(entry.Key, entry.Value);
        }
       
        return tmp;
    }

    public void displayMessage(string message)
    {
        if (_dialogueManager == null)
            return;

        _dialogueManager.ShowMessage(message);
    }
    
    public static ParameterUI Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<ParameterUI>();
            return _instance;
        }
    }

    /// <summary>
    /// Resets the object
    /// </summary>
    public void ResetObject()
    {
        Debug.Log("Reset Parameter UI\n");
        Debug.Log("Reset Parameter UI - load default\n");
        showLabelToggle_.isOn = true;
        showOriginGrid_.isOn = true;
        //loadDefault();
    }
}