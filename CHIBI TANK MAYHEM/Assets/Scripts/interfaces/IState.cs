
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
	ChibiSoldierCapturingTarget
}
