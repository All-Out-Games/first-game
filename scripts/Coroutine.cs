using AO;

using System;
using System.Collections;
using System.Collections.Generic;

interface IYield
{
    bool TickYield();
}

public struct WaitForSeconds : IYield
{
    public float Seconds;

    public WaitForSeconds(float seconds)
    {
        Seconds = seconds;
    }

    public bool TickYield()
    {
        Seconds -= Time.DeltaTime;
        return Seconds > 0;
    }
}

public class Coroutine : IYield
{
    public static List<Coroutine> ActiveCoroutines = new();

    public Entity Owner;
    public IEnumerator Enumerator;
    public bool Finished;

    public bool TickYield()
    {
        return !Finished;
    }

    public static Coroutine Start(Entity owner, IEnumerator enumerator)
    {
        var coroutine = new Coroutine() { Owner = owner, Enumerator = enumerator };
        ActiveCoroutines.Add(coroutine);
        return coroutine;
    }

    public static bool Timer(ref float timer, float seconds)
    {
        timer += Time.DeltaTime;
        return timer < seconds;
    }
}

public static class CoroutineExtensions
{
    public static bool Alive(this Coroutine c)
    {
        if (c == null) return false;
        if (c.Finished) return false;
        return true;
    }
}

public class CoroutineSystem : System<CoroutineSystem>
{
    public override void Update()
    {
        for (int i = 0; i < Coroutine.ActiveCoroutines.Count; i++)
        {
            Coroutine coroutine = Coroutine.ActiveCoroutines[i];

            if (!coroutine.Owner.Alive())
            {
                goto end_coroutine;
            }

            if (coroutine.Enumerator.Current != null && coroutine.Enumerator.Current is IYield inst)
            {
                if (inst.TickYield())
                {
                    continue;
                }
            }

            if (coroutine.Enumerator.MoveNext())
            {
                continue;
            }

            end_coroutine:
            coroutine.Finished = true;
            Coroutine.ActiveCoroutines[i] = Coroutine.ActiveCoroutines[Coroutine.ActiveCoroutines.Count-1];
            Coroutine.ActiveCoroutines.RemoveAt(Coroutine.ActiveCoroutines.Count-1);
            i -= 1;
        }
    }
}