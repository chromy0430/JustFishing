[System.Serializable]
public class FishInstance
{
    public FishData fishData;
    public float    length;   // cm
    public float    weight;   // kg
    public int      price;    // 최종 가격

    public FishInstance(FishData data)
    {
        fishData = data;
        length   = data.GetRandomLength();
        weight   = data.GetRandomWeight();
        price    = data.CalculatePrice(length, weight);
    }
    
    public void OverrideValues(float l, float w, int p)
    {
        length = l;
        weight = w;
        price  = p;
    }
}