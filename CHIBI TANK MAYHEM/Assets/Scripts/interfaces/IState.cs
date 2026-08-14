using System;

public interface IState
{
    void Enter();
	void Update();
	void Exit();
	void HandleEvent(NPCEvents evt, object data);
}

public enum NPCEvents
{
    ChibiSoldierFoundTarget,
	ChibiSoldierHasReachedTarget,
	ChibiSoldierCapturingTarget,
}

public static class PlayerEvents
{
	public static Action PlayerTankDeath;
	public static Action<float> PlayerTankBodyTakesDamage;
	public static Action<float> PlayerTankHeadTakesDamage;
	public static Action<float> PlayerTankTurretTakesDamage;
	public static Action<float , int> PlayerTankTrailTakesDamage;
	public static Action PlayerTankTurretDestroyed;
	public static Action<int> PlayerTankTrailDestroyed;

	public static Action<float> PlayerTankBodyHeals;
	public static Action<float> PlayerTankHeadHeals;
	public static Action<float> PlayerTankTurretHeals;
}
