using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Perso2D : MonoBehaviour
{
    // Start is called before the first frame update
    public float vitesse_course = 10;
    public float vitesse_saut =20;
    float vitesse_y_actuelle;
    float vitesse_x_actuelle;
    float vitesse_z_actuelle;
    public float gravité = 10;
    
    void Start()
    {
        vitesse_x_actuelle = 0;
        vitesse_y_actuelle = 0;
        vitesse_z_actuelle = 0;
    }

    // Update is called once per frame
    

    void Update()
    {
        //courir

        if (Input.GetKey(KeyCode.LeftArrow)) vitesse_x_actuelle = -vitesse_course;
        else if (Input.GetKey(KeyCode.RightArrow)) vitesse_x_actuelle = vitesse_course;
        else vitesse_x_actuelle = 0;

        if (Input.GetKey(KeyCode.DownArrow)) vitesse_z_actuelle = -vitesse_course;
        else if (Input.GetKey(KeyCode.UpArrow)) vitesse_z_actuelle = vitesse_course;
        else vitesse_z_actuelle = 0;

        //tester contact avec le sol
        bool contact_sol = gameObject.transform.position.y <= 0;

        //gravité
        if (!contact_sol) vitesse_y_actuelle -= gravité * Time.deltaTime;
        else
        {
            vitesse_y_actuelle = 0;

            Vector3 copie_position = gameObject.transform.position;
            copie_position.y = 0;
            gameObject.transform.position = copie_position;

        }
        //saut
        if (Input.GetKey(KeyCode.Space) && contact_sol) vitesse_y_actuelle = vitesse_saut;
        
        //appliquer les mouvements
        gameObject.transform.Translate(vitesse_x_actuelle * Time.deltaTime, vitesse_y_actuelle* Time.deltaTime, vitesse_z_actuelle * Time.deltaTime);
    }
}
