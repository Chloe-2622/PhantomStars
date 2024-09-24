using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResistanceManager : MonoBehaviour
{
    public static ResistanceManager Instance { get; private set; }

    public enum ResistanceState {
        IDLE,
        REGENERATING,
        RESISTING,
        HARDRESISTING
    }

    [Header("Parameters")]
    [SerializeField] private float resistance = 50f;
    [SerializeField] private float maxResistance = 100f;
    [SerializeField] private float resistanceRate = 10f;

    [Header("Courant")]
    public GameObject courant;

    private ResistanceState resistanceState = ResistanceState.REGENERATING;

    private void Awake() {
        Instance = this;
    }

    private void Update() {
        UpdateResistance();
    }

    public void ChangeResistanceState(ResistanceState newState) {
        resistanceState = newState;

        switch (resistanceState) {
            case ResistanceState.IDLE:
                resistance = 50f;
                resistanceRate = 0f;
                break;
            case ResistanceState.REGENERATING:
                resistanceRate = -10f;
                break;
            case ResistanceState.RESISTING:
                resistanceRate = 5f;
                break;
            case ResistanceState.HARDRESISTING:
                resistanceRate = 20f;
                break;
        }
    }

    private void UpdateResistance() {
        resistance += resistanceRate * Time.deltaTime;
        resistance = Mathf.Clamp(resistance, 0, maxResistance);
        if (resistance == maxResistance) {
            Debug.Log("The fish has escaped!");
        }
    }
}
