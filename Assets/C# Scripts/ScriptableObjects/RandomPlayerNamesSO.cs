using UnityEngine;



[CreateAssetMenu(fileName = "Funny Names", menuName = "ScriptableObjects/AutoNamesSO", order = -1000)]
public class RandomPlayerNamesSO : ScriptableObject
{
    
    [SerializeField] private string[] funnyNames = new string[]
    {
        "JohnDoe",
        "WillowWilson",
        "BijnaMichael",
        "Yi-Long-Ma",
        "Loading4Ever",
        "DickSniffer",
        "CraniumSnuiver",
        "Moe-Lester",
        "HonkiePlonkie",
        "WhyIsThisHere",
        "TheFrenchLikeBaguette",
    };

    public string GetRandomFunnyName()
    {
        return funnyNames.SelectRandom();
    }
}