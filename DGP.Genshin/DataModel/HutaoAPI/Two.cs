namespace DGP.Genshin.DataModel.HutaoAPI
{
    public class Two<T> 
    {
        public Two(T first, T second)
        {
            First = first;
            Second = second;
        }

        public T First { get; set; }
        public T Second { get; set; }
    }

}
