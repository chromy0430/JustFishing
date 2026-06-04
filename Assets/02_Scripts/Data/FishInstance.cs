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
}