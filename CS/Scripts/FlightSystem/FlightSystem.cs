/// <summary>
/// Flight system. This script is Core plane system
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// included all necessary component
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(DamageManager))]
[RequireComponent(typeof(WeaponController))]



public class FlightSystem : MonoBehaviour
{
    public struct SyncData
    {
        //天空
        public float Speed;
        public float throttle;
        public Quaternion mainRot;
        public bool EffectChangeEnable;
        public Vector2 rollPitchAxis;
        //地面
        public bool LandGearOpen;
        public float LandTurnSpeed;
        public float Turn;
        //AI
        //public bool FollowTarget;
        //public Vector3 PositionTarget;

        public SyncData(float speed, float throttle, Quaternion mainRot, bool effectChangeEnable,Vector2 rollPitchAxis, bool landGearOpen, float landTurnSpeed, float trun /*,bool followTarget, Vector3 positionTarget*/)
        {
            Speed = speed;
            this.throttle = throttle;
            this.mainRot = mainRot;
            EffectChangeEnable = effectChangeEnable;
            this.rollPitchAxis = rollPitchAxis;
            LandGearOpen = landGearOpen;
            LandTurnSpeed = landTurnSpeed;
            Turn = trun;
            //FollowTarget = followTarget;
            //PositionTarget = positionTarget;

        }

        //public static bool operator!=(SyncData lData,SyncData rData)
        //{
        //    return !(lData == rData);
        //}

        //public static bool operator ==(SyncData lData, SyncData rData)
        //{
        //    if ( Mathf.Abs(lData.Speed-rData.Speed) > 0.1f)
        //        return false;
        //    if (Mathf.Abs(lData.throttle - rData.throttle) > 0.01f)
        //        return false;
        //    if (lData.mainRot != rData.mainRot)
        //        return false;
        //    if (lData.EffectChangeEnable != rData.EffectChangeEnable)
        //        return false;
        //    if (lData.rollPitchAxis != rData.rollPitchAxis)
        //        return false;
        //    if (!lData.LandGearOpen != !rData.LandGearOpen)
        //        return false;
        //    if(Mathf.Abs(rData.LandGearOpen))
        //    return true;
        //}
    }


    public bool AerodynamicSimulationEnable = true;
    public float Speed = 120.0f;// Speed
	//public float SpeedMax = 60.0f;// Max speed
	public float RotationSpeed = 50.0f;// Turn Speed
    //public float SpeedPitch = 3.0f; //rotation X
	//public float SpeedRoll = 1.0f; // rotation Z
    //public float SpeedYaw = 0.2f; //rotation Y
	public float DampingTarget = 10.0f;// rotation speed to facing to a target
	public bool AutoPilot = false;// if True this plane will follow a target automatically
	//private float MoveSpeed = 10;// normal move speed
    //new
    public float Sensitive = 1.0f; //灵敏度
    public float MaxRollSpeed = 3.0f; //最大滚转速率
    public float MaxPitchSpeed = 1.6f; //最大俯仰速率
    public float MaxYawSpeed = 0.4f; //最大偏航速率

    public float Weight = 62.6f;
    public float GravityAcc = 10.0f;//重力加速度
    
    [HideInInspector]
    public float Resistance; //阻力 = r0 + r_coef*v*v
    public float ResistanceCoef = 0.1f; //阻力系数
    public float Resistance0 = 200.0f;
    [HideInInspector]
    public float Brake = 1.0f;//刹车
    public float EnginePower = 2760.0f; //发动机推力
    [HideInInspector]
    public float AfterBurner = 1.0f; //加力
    [HideInInspector]
    public bool AfterBurnerOn = false;//是否在开加力
    [HideInInspector]
    public float AfterBurnerRecover = 10.0f;//加力持续时间

    [HideInInspector]
    public bool Stall = false;
    public float StallSpeed = 35.0f; //失速速度
    public float StallAngle = 30.0f; //失速迎角
    //public float LiftWorkMinThrottle = 0.2f;//0~1.25
    public float LiftWorkMinSpeed = 83;
    public float WingAngle0 = 3.0f; //机翼初始迎角
    public float LiftCoef = 1.0f; //升力系数

    public List<WheelCollider> TurnWheels = new List<WheelCollider>();
    public List<GameObject> TurnWheelsObjs = new List<GameObject>();
    public List<WheelCollider> NormalWheels = new List<WheelCollider>();
    public List<GameObject> NeedHideLandGears = new List<GameObject>();

    //[HideInInspector]
    //public bool Locked = true;
    [HideInInspector]
	public bool SimpleControl = false;// set true is enabled casual controling //已废除
	[HideInInspector]
	public bool FollowTarget = false;
	[HideInInspector]
	public Vector3 PositionTarget = Vector3.zero;// current target position
	[HideInInspector]
	public DamageManager DamageManage;
	[HideInInspector]
	public WeaponController WeaponControl;// weapon system
	private Vector3 positionTarget = Vector3.zero;
    [HideInInspector]
    public Quaternion mainRot = Quaternion.identity;    //当前机身的姿态朝向
	[HideInInspector]
	public float Aileron = 0;
	[HideInInspector]
	public float Elevator = 0;
	[HideInInspector]
	public float Rudder = 0;
    [HideInInspector]
    public float roll = 0;
    [HideInInspector]
    public float pitch = 0;
    [HideInInspector]
    public float yaw = 0;
    //[HideInInspector]
    public float throttle = 0.5f; //油门大小，范围为0至1
    public bool EffectChangeEnable = true;   //是否启用外观特效
    //public Vector2 LimitAxisControl = new Vector2 (2, 1);// limited of axis rotation magnitude
	public bool FixedX;//没用？
	public bool FixedY;
	public bool FixedZ;
	public float FlyingMess = 30;  //质量
    public float LandingMess = 120f;
    //float MessTranlateSpeed = 1f;
    float MessValue;
    public bool DirectVelocity = true;//已废除// if true this riggidbody will not receive effect by other force.
	public float DampingVelocity = 5;
    public int bound_x;
    public int bound_z;
    public int bound_y;
    

    public ParticleSystem []JetEffects;
    public AudioSource EngineSound1;
    public AudioSource EngineSound2;

    public bool LandGearOpen = true;
    private bool SwitchLandgearPeriod = false;
    public float LandTurnSpeed = 5f;

    private Animator animator;

    //气动特效
    public float PitchHeadEffectsMaxStartLiftTime = 1.5f;
    public List<ParticleSystem> PitchHeadEffects = new List<ParticleSystem>();
    public float PitchWingEffectsMaxStartLifeTime = 0.7f;
    public List<ParticleSystem> PitchWingEffects = new List<ParticleSystem>();
    public float WingLineEffectsMaxTime = 4;
    public List<TrailRenderer> WingLineEffects = new List<TrailRenderer>();

    private void Awake()
    {
        // define all component
        animator = gameObject.GetComponentInChildren<Animator>();
        DamageManage = this.gameObject.GetComponent<DamageManager>();
        WeaponControl = this.gameObject.GetComponent<WeaponController>();
    }

    void Start ()
	{
		
		mainRot = this.transform.rotation;
		GetComponent<Rigidbody>().mass = FlyingMess;
        MessValue = LandGearOpen ? LandingMess : FlyingMess;
        lastLangear = LandGearOpen;
        GetComponent<Rigidbody>().velocity = (GetComponent<Rigidbody>().rotation * Vector3.forward) * Speed;    //初始速度相关
        foreach(ParticleSystem item in JetEffects)
            item.Play();
        //初始化起落架
        if (animator)
        {
            animator.SetBool("LandGearOpen", LandGearOpen);
            if (!LandGearOpen)
                foreach (var item in NeedHideLandGears)
                    item.SetActive(LandGearOpen);
        }
        //Locked = false;
        EffectChange();
    }

    void FixedUpdate()
    {
        isground = hasWheelOnGround();
        AerodynamicSimulation();
        EffectChange();
        LandGearGanged();
    }

    bool lastLangear = false;
    private void LandGearGanged()
    {
        if (!animator)
            return;

        bool onGround = isGround;

        if (LandGearOpen || !LandGearOpen && onGround)
        {
            if (lastLangear != LandGearOpen)
            {
                LandGearOpen = true;
                animator.SetBool("LandGearOpen", true);
                foreach (var item in NeedHideLandGears)
                    item.SetActive(true);
            }
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0)
                SwitchLandgearPeriod = true;
            else
                SwitchLandgearPeriod = false;
        }
        else
        {
            if (lastLangear != LandGearOpen)
            {
                animator.SetBool("LandGearOpen", false);
                foreach (var item in NeedHideLandGears)
                    item.SetActive(false);
            }
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0)
                SwitchLandgearPeriod = true;
            else
                SwitchLandgearPeriod = false;
            //StartCoroutine(WaitForWheelSetActive(animator.GetCurrentAnimatorStateInfo(0).length, NeedHideLandGears, false));
        }
        lastLangear = LandGearOpen;
    }

    public bool isGround { get => isground; }
    private bool isground;
    public bool hasWheelOnGround()
    {
        foreach (var item in NormalWheels)
        {
            if (!item.gameObject.activeSelf)
                continue;
            if (item.isGrounded)
                return true;
        }
        foreach (var item in TurnWheels)
        {
            if (!item.gameObject.activeSelf)
                continue;
            if (item.isGrounded)
                return true;
        }
        return false;
    }

    void AerodynamicSimulation()
    {
        if (!this.GetComponent<Rigidbody>() || !AerodynamicSimulationEnable)
        {
            //加上刹车
            foreach (var item in NormalWheels)
                item.brakeTorque = 100000;
            return;
        }
        //解除刹车
        foreach (var item in NormalWheels)
            item.brakeTorque = 0f;

        Quaternion AddRot = Quaternion.identity;
        //Vector3 velocityTarget = Vector3.zero;
        Quaternion VelocityRot = Quaternion.identity;

        Vector3 Lift = Vector3.zero;//升力
        Vector3 Tail = Vector3.zero;//垂直尾翼的作用
        Vector3 Drag = Vector3.zero;//阻力
        Vector3 Push = Vector3.zero;//推力
        Vector3 velocity = GetComponent<Rigidbody>().velocity; //速度矢量
        //MessValue = Mathf.Lerp(MessValue, LandGearOpen ? LandingMess : FlyingMess, 10 * Time.fixedDeltaTime);
        float gravityMess = AutoPilot || !LandGearOpen ? FlyingMess : MessValue;
        Vector3 Gravity = -gravityMess * GravityAcc * Vector3.up;
        Vector3 bound_position = GetComponent<Rigidbody>().transform.position;
        if (bound_position.x > bound_x)
        {
            bound_position.x = bound_x;
        }
        if (bound_position.x < -bound_x)
        {
            bound_position.x = -bound_x;
        }
        if (bound_position.y > bound_y)
        {
            bound_position.y = bound_y;
        }
        if (bound_position.y < -bound_y)
        {
            bound_position.y = -bound_y;
        }
        if (bound_position.z > bound_z)
        {
            bound_position.z = bound_z;
        }
        if (bound_position.z < -bound_z)
        {
            bound_position.z = -bound_z;
        }
        GetComponent<Rigidbody>().transform.position = bound_position;
        if (AutoPilot)
        {// if auto pilot
            if (FollowTarget)
            {
                // rotation facing to the positionTarget
                positionTarget = Vector3.Lerp(positionTarget, PositionTarget, Time.fixedDeltaTime * DampingTarget);
                Vector3 relativePoint = this.transform.InverseTransformPoint(positionTarget).normalized;
                mainRot = Quaternion.LookRotation(positionTarget - this.transform.position);
                GetComponent<Rigidbody>().rotation = Quaternion.Lerp(GetComponent<Rigidbody>().rotation, mainRot, Time.fixedDeltaTime * (RotationSpeed * 0.01f));
                this.GetComponent<Rigidbody>().rotation *= Quaternion.Euler(-relativePoint.y * 2, 0, -relativePoint.x * 10);

            }
            velocity = (GetComponent<Rigidbody>().rotation * Vector3.forward) * Speed;
            //velocityTarget = (GetComponent<Rigidbody>().rotation * Vector3.forward) * Speed;
            if (DirectVelocity)
            {
                GetComponent<Rigidbody>().velocity = velocity;
            }
            else
            {
                GetComponent<Rigidbody>().velocity = Vector3.Lerp(GetComponent<Rigidbody>().velocity, velocity, Time.fixedDeltaTime * DampingVelocity);
            }
        }
        else
        {
            //if (throttle * AfterBurner > 0.25f)
            //{
            //GetComponent<Rigidbody>().useGravity = false;
            //玩家操控时



            //姿态控制
            //{
            //VelocityRot.eulerAngles = velocity;//
            roll = Aileron;
            pitch = (3.1416f * (Vector3.Angle(velocity, GetComponent<Rigidbody>().rotation * Vector3.up) - 90) / 180.0f + Elevator);
            yaw = -(3.1416f * (Vector3.Angle(velocity, GetComponent<Rigidbody>().rotation * Vector3.right) - 90) / 180.0f - Rudder);
            //print(GetComponent<Rigidbody>().rotation.eulerAngles.y);

            if (SwitchLandgearPeriod||isGround && Speed < LiftWorkMinSpeed/*AfterBurner * throttle < LiftWorkMinThrottle*/)
            {
                //GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                //bool isLand = false;
                //foreach (var item in TurnWheels)
                //{
                //    if (item.isGrounded)
                //    {
                //        isLand = true;
                //        break;
                //    }
                //}
                if (isGround && Speed > 0.5)
                {
                    float angle = 0;
                    if (Mathf.Abs(Turn-0)>0.1)
                        angle = TurnWheels[0].transform.localEulerAngles.y + 90 > 360 ? -1 : 1;
                    transform.Rotate(transform.rotation * transform.up * angle * LandTurnSpeed * Time.fixedDeltaTime);
                }

                //GetComponent<Rigidbody>().MoveRotation(transform.rotation);
                mainRot = transform.rotation;   //同步角度
                //GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            }
            else   //产生偏向力
            {
                AddRot.eulerAngles = new Vector3(Speed > StallSpeed ? pitch : pitch * Vector3.Dot(velocity.normalized, transform.forward), yaw, Speed > StallSpeed ? -roll : -roll * Vector3.Dot(velocity.normalized, transform.forward));
                mainRot *= AddRot;
                GetComponent<Rigidbody>().rotation = Quaternion.Lerp(GetComponent<Rigidbody>().rotation, mainRot, Time.fixedDeltaTime * RotationSpeed);

            }
            //if (!LandGearOpen||AfterBurner * throttle >= LiftWorkMinThrottle)   //产生偏向力
            //{
            //    AddRot.eulerAngles = new Vector3(Speed > StallSpeed ? pitch : pitch * Vector3.Dot(velocity.normalized, transform.forward), yaw, Speed > StallSpeed ? -roll : -roll * Vector3.Dot(velocity.normalized, transform.forward));
            //    mainRot *= AddRot;
            //    if(Quaternion.Angle(GetComponent<Rigidbody>().rotation, mainRot) > 0.1f)
            //        GetComponent<Rigidbody>().rotation = Quaternion.Lerp(GetComponent<Rigidbody>().rotation, mainRot, Time.fixedDeltaTime * RotationSpeed);
            //}
            //else if(LandGearOpen)
            //{
            //    GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            //    bool isLand = false;
            //    foreach (var item in TurnWheels)
            //    {
            //        if(item.isGrounded)
            //        {
            //            isLand = true;
            //            break;
            //        }
            //    }
            //    if(isLand&&Speed>1)
            //    {
            //        float angle = 0;
            //        if(TurnWheels[0].transform.localEulerAngles.y!=0) 
            //            angle=TurnWheels[0].transform.localEulerAngles.y + 90 > 360 ? -1 : 1;
            //        transform.Rotate(transform.rotation * transform.up * angle * LandTurnSpeed * Time.fixedDeltaTime);
            //    }
            //    GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            //    //AddRot.eulerAngles = new Vector3(0, yaw, 0);
            //}
            //mainRot *= AddRot;
            //GetComponent<Rigidbody>().rotation = Quaternion.Lerp(GetComponent<Rigidbody>().rotation, mainRot, Time.fixedDeltaTime * RotationSpeed);


            //mainRot = transform.rotation;
            //mainRot = VelocityRot * AddRot;


            //}
            //Speed = velocity.magnitude;
            //print(velocity + "\t" + transform.forward + "\t" + Vector3.Dot(velocity.normalized, transform.forward));
            Speed = velocity.magnitude * Vector3.Dot(velocity.normalized, transform.forward);

            //升力计算
            //{
            //失速判断
            float WingAngle = WingAngle0 + Vector3.Angle(velocity, GetComponent<Rigidbody>().rotation * Vector3.up) - 90.0f; //机翼迎角
            if (Speed < StallSpeed)
            {
                Lift = Vector3.zero;
                if (Speed >= LiftWorkMinSpeed)
                    Stall = true;
            }
            else if (Mathf.Abs(WingAngle) > StallAngle)
            {
                Lift = -10.0f * ResistanceCoef * Speed * Speed * velocity.normalized;
                if (Speed >= LiftWorkMinSpeed)
                    Stall = true;
            }
            else
            {
                Lift = GetComponent<Rigidbody>().rotation * Vector3.up * LiftCoef * Speed * Speed * (WingAngle * 3.1416f / 180.0f);
                Stall = false;
            }

            //推力计算
            //{
            Push = AfterBurner * EnginePower * ((LandGearOpen&&throttle>0.5)?0.5f:throttle) * (GetComponent<Rigidbody>().rotation * Vector3.forward);
            //}

            //垂直尾翼的作用
            //{
            float TailAngle = Rudder + Vector3.Angle(velocity, GetComponent<Rigidbody>().rotation * Vector3.right) - 90.0f;
            Tail = GetComponent<Rigidbody>().rotation * Vector3.right * LiftCoef / 20.0f * Speed * Speed * (TailAngle * 3.1416f / 180.0f);
            //}

            //阻力计算
            //{
            Resistance = Brake * Resistance0 + ResistanceCoef * Speed * Speed;
            Drag = -Resistance * velocity.normalized;
            //}



            //动力学方程
            //{
            velocity += (Lift + Tail + Drag + Push + Gravity) / Weight * Time.fixedDeltaTime;

            //if(AfterBurner*throttle!=0/*&&Vector3.Distance(GetComponent<Rigidbody>().velocity,velocity)>1f*/)

            GetComponent<Rigidbody>().velocity = velocity;

            //if (AfterBurner * throttle < LiftWorkMinThrottle)
            //{
            //    GetComponent<Rigidbody>().useGravity = true;
            //    GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY;
            //    if (flg)
            //    {
            //        GetComponent<Rigidbody>().constraints = GetComponent<Rigidbody>().constraints | RigidbodyConstraints.FreezePositionY;
            //    }
            //}
            //else if (AfterBurner * throttle > LiftWorkMinThrottle && velocity.normalized.y > 0.5f)
            //{
            //    GetComponent<Rigidbody>().constraints |= GetComponent<Rigidbody>().constraints ^ RigidbodyConstraints.FreezePositionY;
            //    if (!flg)
            //    {
            //        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            //        GetComponent<Rigidbody>().useGravity = false;
            //    }
            //}

            //}
            //}
            //else
            //{
            ////    GetComponent<Rigidbody>().velocity = Vector3.zero;
            ////    GetComponent<Rigidbody>().rotation = transform.rotation;
            //    GetComponent<Rigidbody>().useGravity = true;
            ////    GetComponent<Rigidbody>().angularDrag = 0.05f;


            //JetEffect.startSize= Mathf.Lerp(0.0f, 2.0f, throttle * AfterBurner / 1.25f);
        }
    }

    private void EffectChange()
    {
        if (!EffectChangeEnable)
        {
            if (PitchHeadEffects.Count > 0 && PitchHeadEffects[0].gameObject.activeSelf)
                foreach (var item in PitchHeadEffects)
                    item.gameObject.SetActive(false);
            if (PitchWingEffects.Count > 0 && PitchWingEffects[0].gameObject.activeSelf)
                foreach (var item in PitchWingEffects)
                    item.gameObject.SetActive(false);

            if (WingLineEffects.Count > 0 && WingLineEffects[0].gameObject.activeSelf)
                foreach (var item in WingLineEffects)
                    item.gameObject.SetActive(false);

            if (EngineSound1.isPlaying)
                EngineSound1.Stop();
            if (EngineSound2.isPlaying)
                EngineSound2.Stop();
            if (JetEffects.Length > 0 && JetEffects[0].gameObject.activeSelf)
                foreach (ParticleSystem item in JetEffects)
                    item.gameObject.SetActive(false);
        }
        else
        {
            if (PitchHeadEffects.Count > 0 && !PitchHeadEffects[0].gameObject.activeSelf)
                foreach (var item in PitchHeadEffects)
                    item.gameObject.SetActive(true);
            if (PitchWingEffects.Count > 0 && !PitchWingEffects[0].gameObject.activeSelf)
                foreach (var item in PitchWingEffects)
                    item.gameObject.SetActive(true);

            if (WingLineEffects.Count > 0 && !WingLineEffects[0].gameObject.activeSelf)
                foreach (var item in WingLineEffects)
                    item.gameObject.SetActive(true);


            if (!EngineSound1.isPlaying)
                EngineSound1.Play();
            if (!EngineSound2.isPlaying)
                EngineSound2.Play();
            //发动机尾焰和声音效果变化
            foreach (ParticleSystem item in JetEffects)
            {
                if (!item.gameObject.activeSelf)
                    item.gameObject.SetActive(true);
                var main = item.main;
                main.startSize = Mathf.Lerp(0.0f, 2.0f, throttle * AfterBurner / 1.25f);
            }
            EngineSound1.pitch = Mathf.Lerp(0.4f, 2.0f, throttle * AfterBurner / 1.25f);
            EngineSound2.pitch = Mathf.Lerp(0.4f, 2.0f, throttle * AfterBurner / 1.25f);
            EffctRollPitch();

            //轮胎变化
            int i = 0;
            while (i < TurnWheels.Count || i < TurnWheelsObjs.Count)
            {
                if (i < TurnWheels.Count)
                {
                    Vector3 elur = TurnWheels[i].transform.localRotation.eulerAngles;
                    elur.y = Turn < 0 ? 360 + Turn : Turn;
                    //TurnWheels[i].transform.localRotation = Quaternion.Euler(elur);
                    TurnWheels[i].transform.localRotation = Quaternion.Lerp(TurnWheels[i].transform.localRotation, Quaternion.Euler(elur), 4f * Time.fixedDeltaTime);
                }
                if (i < TurnWheelsObjs.Count)
                {
                    Vector3 elur = TurnWheelsObjs[i].transform.localRotation.eulerAngles;
                    elur.y = Turn < 0 ? 360 + Turn : Turn;
                    //TurnWheelsObjs[i].transform.localRotation = Quaternion.Euler(elur);
                    TurnWheelsObjs[i].transform.localRotation = Quaternion.Lerp(TurnWheelsObjs[i].transform.localRotation, Quaternion.Euler(elur), 4f * Time.fixedDeltaTime);
                }
                i++;
            }
        }
    }


    Vector2 rollPitchAxis = Vector2.zero;
    // Input function. ( roll and pitch)
    public void RollPitchControl (Vector2 axis)
    {

        rollPitchAxis = axis;
        Aileron = Mathf.Clamp(axis.x * Sensitive * MaxRollSpeed, -MaxRollSpeed, MaxRollSpeed); //滚转控制
        if (animator)
            animator.SetFloat("Roll", axis.x);

        Elevator = Mathf.Clamp(axis.y * Sensitive * MaxPitchSpeed, -MaxPitchSpeed, MaxPitchSpeed); //俯仰控制
        if (animator)
            animator.SetFloat("Pitch", axis.y);

        
        //EffctRollPitch();
    }

    private void EffctRollPitch()
    {
        foreach (var item in WingLineEffects)
            if (Vector2.Distance(rollPitchAxis, Vector2.zero) > 0.1f)
                item.time = WingLineEffectsMaxTime * Vector2.Distance(rollPitchAxis, Vector2.zero);
            else
                item.time = -1;

        float v = Mathf.Abs(rollPitchAxis.y);
        foreach (ParticleSystem tiem in PitchHeadEffects)
        {
            var main = tiem.main;
            main.startLifetime = PitchHeadEffectsMaxStartLiftTime * (v > 0.4 && Speed > 100/*throttle * AfterBurner > 0.5*/ ? v : 0);
        }
        foreach (ParticleSystem tiem in PitchWingEffects)
        {
            var main = tiem.main;
            main.startLifetime = PitchWingEffectsMaxStartLifeTime * (v > 0.9 && Speed > 100/*throttle * AfterBurner > 0.5*/ ? v : 0);
        }
    }


    //float lateYaw = 0f;
    // Input function ( yaw) 
    public void YawControl (float yaw)
	{
        float YawGain = Sensitive * 60 * Time.fixedDeltaTime;
        Rudder = Mathf.Clamp(yaw * YawGain, -MaxYawSpeed, MaxYawSpeed); //方向舵控制
        //if(Mathf.Abs(lateYaw-yaw)>0.01)
            //lateYaw=Mathf.Lerp(lateYaw,yaw,)
        if(animator)
            animator.SetFloat("Yaw", yaw);

    }
    /// <summary>
    /// Speed up 油门挡位控制
    /// </summary>
    /// <param name="delta">输入坐标轴值</param>
    public void SpeedUp (float delta)
	{
        
        if(delta > 0)
        {
            throttle += Time.fixedDeltaTime * 0.05f;
        }
        else if(delta < 0)
        {
            throttle -= Time.fixedDeltaTime * 0.05f;
        }
        
        throttle = Mathf.Clamp(throttle, 0, 1);
    }

    public void SensitiveControl(float sensitive)
    {
        if (sensitive > 0) Sensitive = 1.0f;
        else if (sensitive < 0) Sensitive = 0.2f;
        else Sensitive = 0.6f;
    }

    public void AfterBurnerControl(float afterburner)
    {
        if (afterburner > 0)
        {
            if(AfterBurnerRecover > 9.0f)
            {
                AfterBurnerOn = true;
                //JetEffect.Play();
            }
            if(AfterBurnerRecover > 0 && AfterBurnerOn)
            {
                AfterBurner = 1.25f;
                AfterBurnerRecover -= Time.fixedDeltaTime;
                //JetEffect.Play();
            }
            else
            {
                AfterBurner = 0.4f;
                AfterBurnerOn = false;
                //JetEffect.Stop();
            }
        }
        else
        {
            if (AfterBurnerRecover < 2.5f) AfterBurner = 0.4f;
            else AfterBurner = 1.0f;

            AfterBurnerRecover += Time.fixedDeltaTime * 0.2f;
            //JetEffect.Stop();
        }
    }

    public void BrakeControl(float brake)
    {
        if (brake > 0) Brake = 4.0f;
        else Brake = 1.0f;
    }

    float Turn = 0;

    public void TurnWheelsControll(float turn)
    {
        Turn = turn;
    }

    public void SwitchLandGear()
    {
        LandGearOpen = !LandGearOpen;
    }

    //IEnumerator WaitForWheelSetActive(float second,List<GameObject> list,bool value)
    //{
    //    SwitchLandgearPeriod = true;
    //    yield return new WaitForSeconds(second);
    //    foreach (var item in list)
    //    {
    //        item.SetActive(value);
    //    }
    //    SwitchLandgearPeriod = false;
    //}

    public SyncData Sync
    {
        get => new SyncData(Speed, throttle, mainRot, EffectChangeEnable, rollPitchAxis , LandGearOpen, LandTurnSpeed,Turn /*,FollowTarget, PositionTarget*/);
        set
        {
            Speed = value.Speed;
            throttle = value.throttle;
            mainRot = value.mainRot;
            EffectChangeEnable = value.EffectChangeEnable;
            rollPitchAxis = value.rollPitchAxis;
            LandGearOpen = value.LandGearOpen;
            LandTurnSpeed = value.LandTurnSpeed;
            Turn = value.Turn;
            //FollowTarget = value.FollowTarget;
            //PositionTarget = value.PositionTarget;
        }
    }

}
