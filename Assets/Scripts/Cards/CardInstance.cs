public class CardInstance
{
    public CardDef def;

    public CardInstance(CardDef d) => def = d;

    public override string ToString() => $"{def.name} (C:{def.cost})";
}
