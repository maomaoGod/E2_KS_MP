using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

public class E2ServerBulletAuthority : ksServerEntityScript
{
    private static Random s_Random = new Random();
    public delegate void OnBulletDead(E2ServerBulletAuthority authority);
    public uint Owner
    {
        get { return m_OwnerId; }
        set
        {
            m_OwnerId = value;
            Properties[Consts.Prop.OWNER] = value;
            Properties[Consts.Prop.ENTITYTYPE] = (int)Consts.EntityType.E_Entity_Bullet;
            
        }
    }
    private uint m_OwnerId;
    public ksIServerPlayer Target
    {
        set
        {
            m_Target = value;
            
        }
    }
    
    private ksIServerPlayer m_Target;
    
    private ksVector3 m_LinearDir;

    public ksVector3 LinearDir
    {
        set
        {
            m_LinearDir = value;
        }
    }
    
    public OnBulletDead BulletDead;
    private float m_fBulletTime;
    
    public override void Initialize()
    {
        Room.OnUpdate[0] += Update;
        Entity.OnOverlapStart += OnOverlap;
        m_fBulletTime = 0.0f;
    }
    
    // Called when the script is detached.
    public override void Detached()
    {
        Room.OnUpdate[0] -= Update;
        Entity.OnOverlapStart -= OnOverlap;
    }
    
    
    private void OnOverlap(ksOverlap overlap)
    {
        // Ignore overlaps with the owner that shot the bullet.
        if (overlap.Entity1.Owner == m_Target)
        {
            ksLog.Info($"E2ServerBulletAuthority.OnOverlap {overlap.Entity1.Owner}");
            BulletDead?.Invoke(this);
            //Entity.Destroy();
        }
    }
    
    // Called during the update cycle
    private void Update()
    {
        var track = (Consts.BulletTrack)Properties[Consts.Prop.BULLETTRACK].Int;
        switch (track)
        {
            case Consts.BulletTrack.E_Track_Follow:
                FollowTrack();
                break;
            case Consts.BulletTrack.E_Track_Linear:
                LinearTrack();
                break;
        }
    }

    private void FollowTrack()
    {
        if(null == m_Target)
            return;

        if (!E2ServerRoomScript.Instance.CachedPlayers.TryGetValue(m_Target, out var plEntity))
        {
            return;
        }
        
        var plPos = plEntity.Transform.Position;
        Transform.LookAt(plPos);
        var tarPos = plEntity.Transform.Position;
        tarPos.Y += 1.7f;
        var dir = (tarPos - Transform.Position).Normalized();
        var delta = 0.1f * dir;
        var pos = Transform.Position;
        pos += delta;
        Transform.Position = pos;
        
        var dist = (tarPos - Transform.Position).Magnitude();
        if (dist < 0.2f)
        {
            ksLog.Info($"E2ServerBulletAuthority.FollowTrack {plEntity}");

            if (null == plEntity.Scripts)
            {
                return;
            }

            try
            {
                var svr = plEntity.Scripts.Get<E2ServerPlayerAuthority>();
                var hp = svr.Entity.Properties[Consts.Prop.HP].Int;
                svr.Entity.Properties[Consts.Prop.HP] = hp - 1;

                BulletDead?.Invoke(this);
            }
            catch (Exception e)
            {
                ksLog.Info($"E2ServerBulletAuthority.FollowTrack {e}");
            }
            
        }
    }

    private void LinearTrack()
    {

        m_fBulletTime += Room.Time.RealDelta;
        if (m_fBulletTime > 7.0F)
        {
            ksLog.Info($"E2ServerBulletAuthority.TimeOut {Properties[Owner]}");
            BulletDead?.Invoke(this);
        }
        else
        {
            var delta = 0.1f * m_LinearDir;
            var pos = Transform.Position;
            pos += delta;
            Transform.Position = pos;
        }
        
    }
}