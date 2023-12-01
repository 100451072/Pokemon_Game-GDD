namespace Pokemon.Moves
{
    public class Move
    {
        public MoveBase Base { get; }

        public int Pp { get; set; }

        public Move(MoveBase pBase)
        {
            Base = pBase;
            Pp = pBase.Pp;
        }
    }
}
