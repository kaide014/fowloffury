using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class HealthDisplay : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] public Health health;
    [SerializeField] public Image healthBarImage;
    public static bool isMovingForward;
    public static bool isMovingBackward;
    public static bool isMovingUp;
    public static bool isMovingDown;
    public static bool isLeftJab;
    public static bool isRightJab;
    public static bool isLeftKick;
    public static bool isRightKick;
    public static bool isParry;
    public static bool isCombo;

    public override void OnNetworkSpawn()
    {
        if (!IsClient) 
        {
            return;
        }

        isMovingForward = false;
        health.CurrentHealth.OnValueChanged += HandleHealthChanged;
        HandleHealthChanged(0, health.CurrentHealth.Value);
    }

    public override void OnNetworkDespawn()
    {
        if (!IsClient) return;

        health.CurrentHealth.OnValueChanged -= HandleHealthChanged;
    }

    public void HandleHealthChanged(int oldHealth, int newHealth)
    {
        healthBarImage.fillAmount = (float)newHealth / Health.MaxHealth;
    }

    public void MoveForwardDown()
    {
        isMovingForward = true;
    }

    public void MoveForwardUp()
    {
        isMovingForward = false;
    }

    public void MoveBackwardDown()
    {
        isMovingBackward = true;
    }

    public void MoveBackwardUp()
    {
        isMovingBackward = false;
    }

    public void MoveUpDown()
    {
        isMovingUp = true;
    }

    public void MoveUpUp()
    {
        isMovingUp = false;
    }

    public void MoveDownDown()
    {
        isMovingDown = true;
        Debug.Log(isMovingDown);
    }

    public void MoveDownUp()
    {
        isMovingDown = false;
        Debug.Log(isMovingDown);
    }

    public void LeftJabAttackUp()
    {
        isLeftJab = true;
    }

    public void LeftJabAttackDown()
    {
        isLeftJab = false;
    }

    public void RightJabAttackUp()
    {
        isRightJab = true;
    }

    public void RightJabAttackDown()
    {
        isRightJab = false;
    }

    public void LeftKickAttackDown()
    {
        isLeftKick = true;
    }

    public void LeftKickAttackUp()
    {
        isLeftKick = false;
    }

    public void RightKickAttackDown()
    {
        isRightKick = true;
    }

    public void RightKickAttackUp()
    {
        isRightKick = false;
    }

    public void IsParryDown()
    {
        isParry = true;
    }

    public void IsParryUp()
    {
        isParry = false;
    }

    public void IsComboDown()
    {
        isCombo = true;
    }

    public void IsComboUp()
    {
        isCombo = false;
    }
}
