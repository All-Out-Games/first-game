using AO;

using System;
using System.Collections;
using System.Collections.Generic;

public class Coroutine
{
    public static List<IEnumerator> ActiveCoroutines = new();

    public static void Start(IEnumerator enumerator)
    {
        ActiveCoroutines.Add(enumerator);
    }

    public static bool WaitForSeconds(ref float timer, float seconds)
    {
        timer += Time.DeltaTime;
        return timer < seconds;
    }
}

public class CoroutineSystem : System<CoroutineSystem>
{
    public override void Update()
    {
        for (int i = 0; i < Coroutine.ActiveCoroutines.Count; i++)
        {
            IEnumerator enumerator = Coroutine.ActiveCoroutines[i];
            if (!enumerator.MoveNext())
            {
                Coroutine.ActiveCoroutines[i] = Coroutine.ActiveCoroutines[Coroutine.ActiveCoroutines.Count-1];
                Coroutine.ActiveCoroutines.RemoveAt(Coroutine.ActiveCoroutines.Count-1);
                i -= 1;
            }
        }
    }
}