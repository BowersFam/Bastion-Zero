/// <summary>
/// Contract for level objects that react to Nexus's sword slash.
/// Implemented by the corrupted bramble barrier and the strike switch.
/// Ordinary body contact never satisfies this contract — only a live
/// <see cref="AttackHitbox2D"/> swing dispatches <see cref="ReceiveAttack"/>.
/// </summary>
public interface IAttackReceiver
{
    void ReceiveAttack();
}
