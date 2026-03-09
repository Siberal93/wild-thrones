using System;

[Serializable]
public class CardDef
{
    public string id;
    public string name;
    public string desc;
    public int cost;

    // per ora: effetto “tipo” + valore (test)
    public string effect;   // es: "DMG_ENEMY", "HEAL_ALLY", "REDUCE_CD"
    public int amount;
}
