using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun.Demo.PunBasics;


public class Gamification : MonoBehaviour
{
    public GameObject[] objetos; //Lista de boosters
    public GameObject botonAccion; //Para activar el booster
    public GameObject botonReflejo; //Para saber si le da a tiempo
    public static GameObject temporal; //Para guardar el objeto que ha tocado
    public int valorObj; //Para seleccionar uno de los booster disponibles
    public int usos = 0; //Para guardar la cantidad de usos que tiene un objeto
    public static int datoAnterior; //Para guardar los ciclos por segundo
                                    //que tenia el jugador antes de activar el booster
    private bool usosDef;
    private bool reflejo;

    /*[ARojo] puedes tomar todas refencias de componentes*/
    private GameManager gameManager;
    private GameObject barco; //Para tener referencia del barco
    private MP_GameManager mpgm;

    public Image miniatura;
    public Sprite[] spriteGas;
    public Sprite[] spriteSlow;
    public Sprite spriteNiebla;

    private void OnEnable()
    {
        gameManager = GetComponent<GameManager>();
        barco = this.gameObject;
        mpgm = FindObjectOfType<MP_GameManager>();
    }


    // Start is called before the first frame updateupdate
    void Start()
    {
        //Desactivamos cualquier objeto que tenga que ver con la activacion y los boosters
        botonAccion.SetActive(false);
        botonReflejo.SetActive(false);
        usos = 0;
        datoAnterior = 0;
        usosDef = false;
        reflejo = false;
    }

    private void Update()
    {
        /*
         La idea es que el boton aparezca en un tiempo aleatorio y se mantenga durante unos segundos. 
         Si se le da al botón, se genera el randomizado de usos (para los modificadores de velocidad) y de booster que se escoge.
         */
        int num = UnityEngine.Random.Range(0, 10);
        if (!reflejo && num % 2 == 0)
        {
            botonReflejo.SetActive(true);
            while(Time.deltaTime < UnityEngine.Random.Range(1, 10));
            botonReflejo.SetActive(false);
        }
        else if (reflejo && !usosDef)
        { 
            usos = generarUsos(usos);
            temporal = generarBooster(temporal);
            usosDef = true;
        }
        else if (usosDef)
        {
            botonAccion.SetActive(true);
        }
        else
        {
            botonAccion.SetActive(false);
        }
    }

    public GameObject generarBooster(GameObject temporal)
    {   
            int objIndex = UnityEngine.Random.Range(0, 8);
            temporal = objetos[objIndex];
            if(objIndex % 3 == 0)
            {
                temporal.tag = "gas";
                miniatura.sprite = spriteGas[usos - 1];
            }
            else if (objIndex % 2 == 0)
            {
                temporal.tag = "banco";
                miniatura.sprite = spriteSlow[usos - 1];
            }
            else
            {
                temporal.tag = "niebla";
                miniatura.sprite = spriteNiebla;
            }
        
        return temporal;
    }
    public int generarUsos(int usos)
    {
        if(usos <= 0)
        {
            usos = UnityEngine.Random.Range(1, 3);
            return usos;
        }
        return usos;
    }
    
    // Permite la activacion del boton para el booster
    public void ActivarBotonBooster()
    {
        reflejo = true;
    }

    //Usar el booster y descontar 1 uso

    public void ActivarBooster()
    {
        if (usos >= 1)
        {
            if (temporal.CompareTag("gas"))
            {
                SpeedUp();
                miniatura.sprite = spriteGas[usos-1];
            }
            else if (temporal.CompareTag("banco"))
            {
                miniatura.sprite = spriteSlow[usos-1];
            }
            else
            {
                miniatura.sprite = spriteNiebla;
            }

            usos --;
        }
        else //Permite la reiteración del algoritmo para el randomizado de usos y el booster
        {
            usosDef = false;
            reflejo = false;
        }
        
    }

   


    //Este metodo toma los ciclos por segundo del jugador
    //Al terminar el temporizador, los ciclos por segundo se restauran a los
    //que estaban antes del booster
    public void SpeedUp()
    {
       datoAnterior = gameManager.currentCycles;
       while (Time.deltaTime < 5)
       {
            gameManager.currentCycles++;
       }
        gameManager.currentCycles=datoAnterior;
    }
}
