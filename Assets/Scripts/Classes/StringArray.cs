[System.Serializable]
public class StringArray
{
    public string[] stringArr;
   
   public StringArray(string[] str)
   {
       stringArr = str;
   }
   public int Length()
   {
       return stringArr.Length;
   }
}