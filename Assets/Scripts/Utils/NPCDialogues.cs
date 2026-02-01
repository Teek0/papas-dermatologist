using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class NPCDialogues
{
    [System.Serializable]
    public class NPCLine
    {
        public string line;


    }
    public string language;

    public List<NPCLine> greetings;
    public List<NPCLine> preambles;
    public List<NPCLine> treatmentRequest;
    public List<NPCLine> pointingAt;
    public List<NPCLine> closing;
    public List<NPCLine> leaving;

    public static NPCDialogues CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<NPCDialogues>(jsonString);
    }
}
