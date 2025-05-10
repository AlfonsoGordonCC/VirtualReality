using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Text;
using System.Timers;
using System;
using System.Diagnostics;
using ArduinoBluetoothAPI;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    #region GM
    //--------DISPLAY DATOS-------
    [Header("Visualizacion")]
    [Space(3)]
    public Text t_velObjetivo;
    public Text t_ciclos;
    public Text t_distance;
    public Text t_velText;
    public Text t_panel_id;
    public Text t_feedback;
    public Text t_duracion;
    public GameObject panel_id;
    public GameObject panel_sesion;
    public GameObject panel_pedaleo;
    public GameObject panel_iniciar;
    //public GameObject panel_estimulacion;
    //public GameObject objetivos;
    public Text params_velObj;
    public Text params_time;
    public Text iniciarText;
    public Text panel_sensor_title;
    //public Text debugText;

    //------BLE CONNECTION PARAM-------
    [Header("BLE Connection")]
    [Space(3)]
    [HideInInspector] public string deviceName;
    public bool bIsConnected = false;
    string received_message;
    [HideInInspector] public string id;
    [HideInInspector] public int i_changeData;

    //----------WiFi Connection-------
    //[Header("WiFI Connection")]
    //public UDPSend udpSend;

    //-----------FEEDBACK-----------
    [Header("Feedback")]
    [Space(3)]
    public bool isEquals = false;
    public bool isHigher = false;
    public bool isLower = false;
    private static Timer aTimer;
    //public GameObject propeller;
    //public GameObject simulacion;

    [Header("Pedalling cycles")]
    [Space(3)]
    //--------------PEDALEO-------------
    public int completedCycles;
    public int currentCycles;// { get; set; }
                             //public int myCycles;
    public bool isCycling;
    private int pastCycles;
    private Vector3 currentAngle;
    private Vector3 targetAngle;

    //-------------VELOCIDAD-----------
    public List<float> listaSpeed = new List<float>();
    public List<int> listaFeedback = new List<int>();
    public float velocidadProm;
    private int g = 0;
    private float distance;
    public float currentTime;
    private float pastTime;
    static float averageSpeed;
    public float velocidadObjetivo;

    [Header("Shader")]
    [SerializeField]
    public Renderer environment;
    public GameObject terrain;
    public GameObject LHand;
    public GameObject RHand;
    //---------MOVIMIENTO--------
    float fewSecs;
    private Stopwatch sw1 = new Stopwatch();
    bool fake_mov_bool;

    //------PROCESSAMIENTO DE DATOS-------
    [Header("Datos")]
    [HideInInspector] public SampleMessageListener messageListener;
    public bool b_KalmanStart;
    public bool isCalib;
    public string cyclesperminuteDCM;

    //-------PARAMETROS CONTROL ------
    int once;
    int n;
    public int myTime;
    public GameObject buttonStart;
    public bool stopListening;
    private BluetoothHelper bluetoothHelper;
    public bool startNow;

    [Header("Feedback")]
    [Space(3)]
    public GameObject pedales;

    public bool objetivoCompletado;
    //public Text final;
    #endregion GM

    #region GAMIFICATION_VARIABLES
    //-----------------------GAMIFICACION---------------
    [Header("Gamificacion")]
    [Space(3)]
    public GameObject botonAccion; //Para activar el booster
    public GameObject botonReflejo; //Para saber si le da a tiempo
    public static GameObject temporal; //Para guardar el objeto que ha tocado
    public int valorObj; //Para seleccionar uno de los booster disponibles
    public int usos = 0; //Para guardar la cantidad de usos que tiene un objeto
    public static int datoAnterior; //Para guardar los ciclos por segundo
                                    //que tenia el jugador antes de activar el booster
    private bool usosDef;
    private bool reflejo;
    private bool goFaster; //Bool para aumentar la velocidad
    private bool goSlower;
    private bool goNiebla;
    private bool revertir;
    private bool boosterIsPressed = false;
    private MP_GameManager mpgm;
    float timerSpeedUp;
    public ParticleSystem granNiebla;
    public ParticleSystem humo;
    private int i_temporal;

    public Image miniatura;
    public Sprite[] spriteGas;
    public Sprite[] spriteSlow;
    public Sprite spriteNiebla;
    public Sprite vacio;

    public Text usosText;
    


    private PhotonView PV;
    [Tooltip("Referencia a este jugador")]
    public static GameObject LocalPlayerInstance;
    #endregion GAMIFICATION_VARIABLES

    #region PRIVATE_MONOBEHAVIOUR_METHODS

        public override void OnEnable()
    {
        PV = GetComponent<PhotonView>();
        miniatura.sprite = vacio;
        mpgm = FindObjectOfType<MP_GameManager>();
        messageListener = GetComponent<SampleMessageListener>();
        humo.Stop();
        granNiebla.Stop();
        goFaster = false;
        goSlower = false;
        goNiebla = false;
        StartCoroutine(ReflejoFadeInOutFlow());
    }

    public override void OnDisable()
    {
        StopAllCoroutines();
    }

    void Start()
    {
        objetivoCompletado = false;
        fake_mov_bool = false;
        revertir = true;
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            //udpSend.init();
            //panel_estimulacion.SetActive(true);
            panel_sesion.SetActive(false);
            panel_id.SetActive(false);
            panel_pedaleo.SetActive(true);
            panel_iniciar.SetActive(false);
            //objetivos.SetActive(false);
        }
        else if (SceneManager.GetActiveScene().buildIndex == 2 || SceneManager.GetActiveScene().buildIndex == 3)
        {
            pedales.SetActive(false);
            panel_sesion.SetActive(false);
            panel_id.SetActive(false);
            panel_pedaleo.SetActive(true);
            panel_iniciar.SetActive(false);
            //objetivos.SetActive(false);
        }
        //Set Elementos Visuales
        startNow = false;
        deviceName = "";

        //simulacion.SetActive(false);
        cyclesperminuteDCM = "";

        //Set Variables
        b_KalmanStart = false;
        bIsConnected = false;
        isCycling = false;
        isEquals = false;
        isLower = false;
        isHigher = false;
        stopListening = false;

        once = 0;
        n = 1;
        g = 0;
        myTime = 1;
        averageSpeed = 1;
        velocidadObjetivo = 4;
        distance = 0;
        this.i_temporal = 0;
        if (SceneManager.GetActiveScene().name == "WeriumStandAloneClouds")
        {
            environment.material.EnableKeyword("noise_speed");
        }
        //Desactivamos cualquier objeto que tenga que ver con la activacion y los boosters
        usos = 0;
        datoAnterior = 0;
        usosDef = false;
        reflejo = false;
        //debugText.text += "Start again |";
        goFaster = false;
        Start_Fake_Mov();
        
    }
    #endregion PRIVATE_MONOBEHAVIOUR_METHODS

    #region ALFONSO_GAMIFICATION_METHODS
    /*
        This method will allow to set the booster and update the sprite.
        We use a random variable and if-else conditional for the choosing.
        Switch not used due to bug appearance.
     */
    public void generarBooster()
    {

        this.i_temporal = UnityEngine.Random.Range(1, 4); //4 is exclusive range

        if (this.i_temporal == 1) //GAS
        {
            miniatura.sprite = spriteGas[usos - 1];
        }
        else if (this.i_temporal == 2) //BANCO PECES
        {
            miniatura.sprite = spriteSlow[usos - 1];
        }
        else
        {
            miniatura.sprite = spriteNiebla; //NIEBLA
        }

    }

    public int generarUsos()    //Generates the uses using a random value between 1 and 3
    {

        if (usos <= 0)
        {
            usos = UnityEngine.Random.Range(1, 4);
            return usos;
        }
        return usos;

    }

    /*
        This methd will happen when the botonReflejo is presed.
     */
    public void ActivarBotonBooster()
    {
        usos = generarUsos();
        generarBooster();
        reflejo = true;
        usosDef = true;
        StopCoroutine(ReflejoFadeInOutFlow());
        botonReflejo.SetActive(false);
        botonAccion.SetActive(true);
        StartCoroutine(BoosterButtonControl());
    }

    /*
        This method activates the action of the booster selected
     */
    public void ActivarBooster()
    {
        if (usos >= 1)
        {

            if (this.i_temporal == 1) //GAS
            {
                SpeedUp();
               // estadoText.text = "Ejecutando Gas";

            }
            else if (this.i_temporal == 2) //BANCO PECES
            {
                PV.RPC("SlowDown", RpcTarget.Others); //Allows to redirect the method SlowDown to the other clients of the room and not to myself
                //estadoText.text = "Ejecutando Slow";
            }
            else //NIEBLA
            {
                PV.RPC("VisionAffected", RpcTarget.Others);
               // estadoText.text = "Ejecutando Niebla";
            }
            boosterIsPressed = true;
        }
    }


    private void actualizarSprites()    //Updates the sprites from miniatura 
    {
        if (usos > 0)
        {
            if (this.i_temporal == 1) //GAS
            {
                miniatura.sprite = spriteGas[usos - 1];

            }
            else if (this.i_temporal == 2) //BANCO PECES
            {
                miniatura.sprite = spriteSlow[usos - 1];
            }
            else //NIEBLA
            {
                miniatura.sprite = spriteNiebla;
            }
        }
    }

    void voverReflejo() //Method activated when there are no more uses left
    {
        StopCoroutine(BoosterButtonControl());
        reflejo = false;
        usosDef = false;
        botonAccion.SetActive(false);
        StartCoroutine(ReflejoFadeInOutFlow());
    }

    IEnumerator ReflejoFadeInOutFlow() //This corroutine allows the apearance and disappearance of the botonReflejo
    {
        botonAccion.SetActive(false);
        while (!reflejo)
        {
            botonReflejo.SetActive(true);
            yield return new WaitForSeconds(UnityEngine.Random.Range(1, 5));
            botonReflejo.SetActive(false);
            yield return new WaitForSeconds(UnityEngine.Random.Range(1, 5));
        }
    }

    //This tag allows al the methods included to be called by anyone from the player of the room

    /*
        Method called when BANCO PECES is activated.
        The method allows to set the variables needed for the cycles update at the Update() method and sets the timer for the countdown.
     */

    [PunRPC]
    public void SlowDown()
    {
        goSlower = true;
        timerSpeedUp = 0;
        revertir = false;
    }

    /*
        Method called when NIEBLA is activated.
        The method starts playing the particle system "granNiebla" and sets the timer for the countdown. 
     */


    [PunRPC]
    public void VisionAffected()
    {
        goNiebla = true;
        timerSpeedUp = 0;
        granNiebla.Play();
    }




    /*
        Method called when GAS is activated.
        Unlike the other methods, this one only activates for the player that casted it. This is why is out from the PunRPC tags
        The method allows to set the variables needed for the cycles update at the Update() method and sets the timer for the countdown. 
     */
    void SpeedUp()
    {
        goFaster = true;
        timerSpeedUp = 0;
        humo.Play();
        revertir = false;
    }


    /*
        This corroutine waits until the boosterBoton is pressed for acting.
        When the button is pressed, all the parameters and graphics are updated.
     */
    IEnumerator BoosterButtonControl()
    {
        do
        {
            yield return new WaitUntil(() => boosterIsPressed == true);
            usos--;
            actualizarSprites();
            boosterIsPressed = false;
        } while (usos > 0);
        resetearSprite();
        voverReflejo();
    }

    void resetearSprite()
    {
        miniatura.sprite = vacio;
        this.i_temporal = 0;
    }

    #endregion ALFONSO_GAMIFICATION_METHODS

    #region PRIVATE_METHODS_IMU_BLE_CONNECTION

    void OnMessageReceived(BluetoothHelper helper)
    {
        received_message = helper.Read();
        //debugText.text = received_message;
        messageListener.SendMessage("OnMessageArrived", received_message);

        if (received_message.Split('=')[0] != "#DCM")
            helper.SendData("#om");
        else if (received_message.Split('=')[0] == "#DCM" && bIsConnected == false)
        {
            bIsConnected = true;
        }
    }

    public static string BinaryToString(string data)
    {
        List<Byte> byteList = new List<Byte>();

        for (int i = 0; i < data.Length; i += 16)
        {
            byteList.Add(Convert.ToByte(data.Substring(i, 16), 2));
        }
        return Encoding.ASCII.GetString(byteList.ToArray());
    }
    void OnConnected(BluetoothHelper helper)
    {
        try
        {
            if (stopListening == false)
                helper.StartListening();
            //debugText.text += "Start Listening |";
        }
        catch (Exception ex)
        {
            //debugText.text += "Listening error |";
            UnityEngine.Debug.Log(ex.Message);
        }
    }
    void OnConnectionFailed(BluetoothHelper helper)
    {
        //debugText.text += "Connection Failed |";
    }
    void AssignID(string device)
    {
        panel_sensor_title.text = "ESPERA MIENTRAS SE CONECTA EL SENSOR";
        //debugText.text += "id Assign |";
        try
        {
            bluetoothHelper = BluetoothHelper.GetInstance(device);
            bluetoothHelper.EnableBluetooth(true);
            bluetoothHelper.OnConnected += OnConnected;
            bluetoothHelper.OnConnectionFailed += OnConnectionFailed;
            bluetoothHelper.OnDataReceived += OnMessageReceived; //read the data

            //debugText.text += "bluetoothHelper name: " + bluetoothHelper.getDeviceName() + " |";
            bluetoothHelper.setTerminatorBasedStream("\n"); //delimits received messages based on \n char
            if (bluetoothHelper != null)
            {
                //debugText.text += "BluetoothHelper != null |";
                if (!bluetoothHelper.isConnected())
                {
                    //debugText.text += "BluetoothHelper isnot connected|";
                    if (bluetoothHelper.isDevicePaired())
                    {
                        StartCoroutine(ConnectBLE(bluetoothHelper));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            //debugText.text += "Catch error |";
            UnityEngine.Debug.Log(ex.Message);
        }
    }
    void StopConnection()
    {
        bluetoothHelper.Disconnect();
        OnDestroy();
        bluetoothHelper = null;
    }
    void OnDestroy()
    {
        if (bluetoothHelper != null)
            bluetoothHelper.Disconnect();

        bluetoothHelper = null;
    }
    public void StartNow()
    {
        buttonStart.SetActive(false);
        iniciarText.fontSize = 35;
        iniciarText.color = Color.cyan;
        StartCoroutine(StartExercise());
    }

    IEnumerator StartExercise()
    {
        iniciarText.text = "3";
        yield return new WaitForSeconds(1f);
        iniciarText.text = "2";
        yield return new WaitForSeconds(1f);
        iniciarText.text = "1";
        startNow = true;
        yield return new WaitForSeconds(1f);
        panel_iniciar.SetActive(false);
        panel_pedaleo.SetActive(true);
        iniciarText.text = "0";
        StopThisCoroutine();
    }

    void StopThisCoroutine()
    {
        StopCoroutine(StartExercise());
    }
    IEnumerator ConnectBLE(BluetoothHelper bluetoothHelper)
    {
        while (bIsConnected == false)
        {
            //debugText.text += "Ble Connect |";
            bluetoothHelper.Connect();
            //debugText.text += "1 |";
            yield return new WaitForSeconds(2f);
            // debugText.text += "2 |";
        }
        StopThisConnection(bluetoothHelper);
    }
    void StopThisConnection(BluetoothHelper bluetoothHelper)
    {
        StopCoroutine(ConnectBLE(bluetoothHelper));
    }

    #endregion PRIVATE_METHODS_IMU_BLE_CONNECTION
   
    #region PRIVATE_METHODS_DATA_FEEDBACK
    private void Update()
    {
        usosText.text = "Usos restantes: " + usos.ToString();
        if (photonView.IsMine)
        {
            
        }
        targetAngle = new Vector3(0f, 0, Time.deltaTime * (3.6f * (float)velocidadObjetivo / 2 * (float)Math.PI * 0.27f * 1000));
        if (fake_mov_bool)
        {
            Movimiento();
        }

        if (goFaster)
        {
            timerSpeedUp += Time.deltaTime;
            if (timerSpeedUp > 5)
            {
                goFaster = false;
                timerSpeedUp = 0;
                revertir = true;
                humo.Stop();

            }
        }

        if (goSlower)
        {
            timerSpeedUp += Time.deltaTime;
            if (timerSpeedUp > 5)
            {
                goSlower = false;
                timerSpeedUp = 0;
                revertir = true;
            }
        }

        if (goNiebla)
        {
            timerSpeedUp += Time.deltaTime;
            if (timerSpeedUp > 5)
            {
                goNiebla = false;
                timerSpeedUp = 0;
                granNiebla.Stop();
            }
        }


        /*if (bIsConnected) 
        {
            panel_id.SetActive(false);           
        }

        if(bIsConnected == true && once < 1)
        {
            panel_iniciar.SetActive(true);
            once++;
        }

        if (isCalib && isCycling && sw1.ElapsedMilliseconds+1000 < myTime*60*1000 && startNow==true)
        {
            Movimiento();
            if (averageSpeed < velocidadObjetivo + 0.5 && averageSpeed > velocidadObjetivo - 0.5)
            {
                isEquals = true;
                isLower = false;
                isHigher = false;
                t_feedback.text = "GENIAL";
            }
            else if (averageSpeed > velocidadObjetivo + 0.5)
            {
                isEquals = false;
                isLower = false;
                isHigher = true;
                t_feedback.text = "REDUCE EL RITMO";
            }
            else if (averageSpeed < velocidadObjetivo - 0.5)
            {
                isEquals = false;
                isLower = true;
                isHigher = false;
                t_feedback.text = "SUBE EL RITMO";
            }
        }

        if (!isCycling )
        {           
            //debugText.text += "is cycling true | ";
            isCycling = true;
        }

        //FIN DEL EJERCICIO;
        if (bIsConnected == true && isCalib && sw1.ElapsedMilliseconds+1000 > myTime * 60 * 1000)
        {
            EndExercise();
            sw1.Stop();
        }*/

        if (SceneManager.GetActiveScene().name == "WeriumStandAloneClouds")
        {
            if (averageSpeed < 10)
            {
                //propeller.transform.Rotate(0, 0, Time.deltaTime * averageSpeed * 50);
                environment.material.SetFloat("Vector1_F17141FF", (float)Math.Floor(averageSpeed) / 10);

            }
            else
            {
                //propeller.transform.Rotate(0, 0, Time.deltaTime * 10 * 50);
                environment.material.SetFloat("Vector1_F17141FF", 1);
            }
        }
        else if (SceneManager.GetActiveScene().name == "WeriumStandAloneShip_2")
        {
            if (averageSpeed < 10)
            {
                //propeller.transform.Rotate(Time.deltaTime * averageSpeed * 0.05f, 0, 0);
            }
            else
            {
                //propeller.transform.Rotate(Time.deltaTime * 10 * 0.05f, 0, 0);
            }
        }
        else if (SceneManager.GetActiveScene().name == "WeriumStandAloneTerrain")
        {
            //TODO instanciar elementos con offset en z y desplezar a v=averageSpeed

            if (averageSpeed < 10)
            {
                //propeller.transform.Rotate(0, 0, Time.deltaTime * averageSpeed * 50);
                terrain.transform.Rotate(Time.deltaTime * averageSpeed * 0.05f, 0, 0);
            }
            else
            {
                //propeller.transform.Rotate(0, 0, Time.deltaTime * 10 * 50);
                terrain.transform.Rotate(Time.deltaTime * 10 * 0.05f, 0, 0);
            }
        }
        //Comprobar si se ha logrado el objetivo en algun momento
        if (averageSpeed > velocidadObjetivo)
        {
            objetivoCompletado = true;
        }
        //Nuevo Fin del Ejercicio
        /*if (sw1.ElapsedMilliseconds + 1000 > myTime * 60 * 1000 && objetivoCompletado)
        {
            EndExercise();
            t_feedback.text = "OBJETIVO COMPLETADO, ENHORABUENA";
            sw1.Stop();
        }
        else if (sw1.ElapsedMilliseconds + 1000 > myTime * 60 * 1000 && !objetivoCompletado)
        {
            EndExercise();
            t_feedback.text = "Objetivo no completado, pero lo has hecho muy bien";
            sw1.Stop();
        }*/
    }
    public int getCurrentCycles()
    {
        return currentCycles;
    }
    public void setCurrentCycles(int a)
    {
        currentCycles = a;

    }
    private void EndExercise()
    {
        if (SceneManager.GetActiveScene().buildIndex == 1)
            //udpSend.sendString("detener");
            SceneManager.LoadScene(0);
        else if (SceneManager.GetActiveScene().buildIndex == 2)
            pedales.SetActive(false);

        t_feedback.text = "FIN DEL EJERCICIO";
        //reset params
        bIsConnected = false;
        isCalib = false;
        isCycling = false;
        SaveCiclesPerMinute();
        startNow = false;

        //saveData User
        MyUser newUser = new MyUser();
        newUser.Name = PlayerPrefs.GetString("UserName");
        newUser.Date = DateTime.Now.ToString();
        newUser.DCM_cycles = cyclesperminuteDCM;
        newUser.Gyro_cycles = "0";
        newUser.TargetTime = (int)myTime;
        newUser.TargetSpeed = (int)velocidadObjetivo;
        SaveSystem.SaveUser(newUser);
        Data_Pedaleo.instance.SaveDatos();
        stopListening = true;
        StopAllCoroutines();
        StartCoroutine(CountDown());
        StopConnection();
    }
    IEnumerator CountDown()
    {
        yield return new WaitForSeconds(2f);
        UnityEngine.SceneManagement.SceneManager.LoadScene("InitialScene");
    }
    public bool firstTime = true;
    public int deltaCycles = 0;
    public void Start_Fake_Mov()
    {
        StartCoroutine("FakeMovimiento");
        fake_mov_bool = true;
    }
    public void Stop_Fake_Mov()
    {
        StopCoroutine("FakeMovimiento");
        fake_mov_bool = false;
    }
    IEnumerator FakeMovimiento()
    {
        while (true)
        {
            int ciclosReales = currentCycles;
            yield return new WaitForSeconds(1f);
            if (goFaster)
            {
                currentCycles += 2;
                ciclosReales++;
            }
            else if (goSlower)
            {
                currentCycles -= 2;
                ciclosReales++;
            }
            else if (revertir && currentCycles != ciclosReales)
            {
                currentCycles = ciclosReales;
                currentCycles++;
            }
            else
            {
                currentCycles++;
            }
        }
    }
    private void Movimiento() //SEGURAMENTE ROMPA
    {
        currentCycles = Data_Pedaleo.instance.cycles;
        currentCycles -= deltaCycles;
        //debugText.text = "Movimiento |";
        if (currentCycles > 0 && !Data_Pedaleo.instance.isStop)
        {
            if (firstTime)
            {
                //debugText.text += "cycles: " + currentCycles.ToString() + " |";
                sw1.Start();
                isCycling = true;
                pastTime = (float)sw1.ElapsedMilliseconds;
                deltaCycles = Data_Pedaleo.instance.cycles;
                firstTime = false;

                /* if (SceneManager.GetActiveScene().buildIndex == 1)
                        udpSend.sendString("abrir");
                    else if (SceneManager.GetActiveScene().buildIndex == 2)
                    {
                        pedales.SetActive(true);
                    }*/
                //myCycles = 1;
            }

            if (goFaster)
            {
                currentCycles++;
            }

            if (currentCycles > pastCycles)
            {
                currentTime = (float)sw1.ElapsedMilliseconds;
                t_ciclos.text = currentCycles.ToString();
                if (SceneManager.GetActiveScene().buildIndex == 1)
                    //udpSend.sendString("abrir");

                    ////CALCULO LA VELOCIDAD DE PEDALEO
                    if (currentCycles > 1)
                    {
                        float numer = ((2.0f * (float)Math.PI) * 0.27f);
                        float denom = (float)(currentTime - pastTime) / 1000;
                        float speed = (numer / denom) * 3.6f;
                        listaSpeed.Add(speed);

                        if (listaSpeed.Count > 8)
                        {
                            float s5 = listaSpeed[g - 5];
                            float s4 = listaSpeed[g - 4];
                            float s3 = listaSpeed[g - 3];
                            float s2 = listaSpeed[g - 2];
                            float s1 = listaSpeed[g - 1];
                            averageSpeed = (s5 + s4 + s3 + s2 + s1) / 5;
                            //Vel Angular (rad/s) = Vel Angular(º/s) * 2PI /360;
                            //Vel Lineal(m/s) = VelAngular (rad/s)*diametro(m); 
                            //Vel Lineal(km/h) = m/s * 3.6;
                            t_velText.text = averageSpeed.ToString("0.00");
                        }
                        else
                        {
                            t_velText.text = speed.ToString("0.00");
                        }

                        distance = numer * currentCycles / 1000;
                        t_distance.text = distance.ToString("0.00");
                        g++;
                        //myCycles++;
                    }
            }
            pastTime = currentTime;
            pastCycles = currentCycles;
        }
    }
    public void SaveCiclesPerMinute()
    {
        cyclesperminuteDCM += currentCycles.ToString() + ",";
        //cyclesperminuteGyro += Data_Pedaleo_Gyro.instance.cycles.ToString() + ",";
    }
    #endregion PRIVATE_METHODS_DATA_FEEDBACK

    #region PUBLIC_BUTTON_EVENTS
    public void GuardarConfigEstimulacion()
    {
        StartCoroutine(ShowPanelSesion());
    }
    public void OKButton()
    {
        deviceName = t_panel_id.text;
        string aux = "202200" + deviceName + "-PM";
        deviceName = aux;
        //debugText.text += deviceName + " |";
        AssignID(deviceName);
    }
    public void Button_1()
    {
        id += 1.ToString();
        t_panel_id.text = id;
    }
    public void Button_2()
    {
        id += 2.ToString();
        t_panel_id.text = id;
    }
    public void Button_3()
    {
        id += 3.ToString();
        t_panel_id.text = id;
    }
    public void Button_4()
    {
        id += 4.ToString();
        t_panel_id.text = id;
    }
    public void Button_5()
    {
        id += 5.ToString();
        t_panel_id.text = id;
    }
    public void Button_6()
    {
        id += 6.ToString();
        t_panel_id.text = id;
    }
    public void Button_7()
    {
        id += 7.ToString();
        t_panel_id.text = id;
    }
    public void Button_8()
    {
        id += 8.ToString();
        t_panel_id.text = id;
    }
    public void Button_9()
    {
        id += 9.ToString();
        t_panel_id.text = id;
    }
    public void Button_0()
    {
        id += 0.ToString();
        t_panel_id.text = id;
    }
    public void Button_dot()
    {
        id += "-PM";
        t_panel_id.text = id;
    }
    public void Button_remove()
    {
        string id_aux = id.Substring(0, id.Length - 1);
        id = id_aux;
        t_panel_id.text = id;
    }
    public void L1()
    {
        velocidadObjetivo -= 1;
        params_velObj.text = velocidadObjetivo.ToString();
    }
    public void L2()
    {
        myTime -= 1;
        params_time.text = myTime.ToString();
    }
    public void M1()
    {
        velocidadObjetivo += 1;
        params_velObj.text = velocidadObjetivo.ToString();
    }
    public void M2()
    {
        myTime += 1;
        params_time.text = myTime.ToString();
    }
    public void OkParams()
    {
        panel_sesion.SetActive(false);
        StartCoroutine(ShowPanelSensor());
        t_velObjetivo.text = velocidadObjetivo.ToString() + " KM/H";
        t_duracion.text = myTime.ToString() + " MIN";
    }

    //Metodo para que cambie de escena a la primera de todas
    public void ExitButton()
    {
        SceneManager.LoadScene(0);
    }


    IEnumerator ShowPanelSensor()
    {
        yield return new WaitForSeconds(1f);
        panel_id.SetActive(true);
        t_panel_id.enabled = true;
        t_panel_id.text = "";
        StopCoroutineShowPanel();

    }
    IEnumerator ShowPanelSesion()
    {
        //panel_estimulacion.SetActive(false);
        yield return new WaitForSeconds(1f);
        panel_sesion.SetActive(true);

        StopCoroutineShowPanelSesion();
    }
    void StopCoroutineShowPanelSesion()
    {
        StopCoroutine("ShowPanelSesion");
    }

    void StopCoroutineShowPanel()
    {
        StopCoroutine("ShowPanelSensor");
    }

    #endregion PUBLIC_BUTTON_EVENTS
}
