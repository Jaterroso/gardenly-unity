﻿using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class ReactProxy : MonoBehaviour
{
    public static ReactProxy instance = null;

    public GraphQL graphQL;
    public ExternalData externalData;
    public Dictionary<string, Action<string>> callbacks = new Dictionary<string, Action<string>>();
    public RawImage image;

    [DllImport("__Internal")]
    private static extern void save(string json);
    [DllImport("__Internal")]
    private static extern void unsavedWork(bool status);
    [DllImport("__Internal")]
    private static extern void query(string payload);

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            graphQL = new GraphQL();
            externalData = this.GetComponent<ExternalData>();
            externalData.Init(callbacks);
            // externalData.callbackFinishDownloadImage = Test;
            if (Application.isEditor)
                DispatchQueryResult("{\"data\":{\"getTypes\":[{\"name\":\"Arbre\",\"id\":\"f025e92f-e115-4cbf-902b-f9551118d2a8\"},{\"name\":\"Arbuste\",\"id\":\"3741e754-c514-4715-a219-732bee92e9e7\"},{\"name\":\"Fleur\",\"id\":\"ca9c6046-38bc-4a2a-a698-0030174d8cbc\"},{\"name\":\"Legume\",\"id\":\"18d21ebd-e124-48b9-83ec-38c888257a02\"}]}}");
            else
                SendQuery(graphQL.GetPlantsTypes());
        }
        else
            Destroy(this);
    }

    private void Start()
    {
        //Fake InitScene
        if (Application.isEditor)
        {
            SerializationController.instance.GetComponent<GardenData>().SetGardenName("Offline Garden");
            LocalisationController.instance.Init("FR");//TODO USERPREF
        }
    }

    //Link To REACT
    public void ExportScene()
    {
        SerializationController.instance.Serialize();
        if (Application.isEditor)
            Debug.Log(SerializationController.instance.GetSerializedData());
        else
            save(SerializationController.instance.GetSerializedData());
    }

    public void UpdateSaveState(bool state)
    {
        if (Application.isEditor)
            return;
        unsavedWork(state);
    }

    public void SendQuery(string payload)
    {
        query(payload);
    }

    //Called from REACT
    public void InitScene(string json)
    {
        if (json != "")
        {
            SerializationController.instance.GetComponent<GardenData>().SetGardenName(JSONObject.Parse(json)["name"]);
            LocalisationController.instance.Init("FR");//TODO USERPREF
            SpawnController.instance.SpawnScene(SerializationController.instance.DeSerialize(json));
        }
    }

    public void DispatchQueryResult(string json)
    {
        var jsonObject = JSONObject.Parse(json);
        if (jsonObject["errors"] != null)
        {
            Debug.Log(jsonObject["errors"].Value);//FIXME
            MessageHandler.instance.ErrorMessage("loading_error");
            return;
        }
        var keys = jsonObject["data"].Keys;
        keys.MoveNext();
        callbacks[keys.Current.Value].Invoke(json);
    }

    //Link to UI
    public string[] GetPlantsType(string plantType)
    {
        if (!externalData.plants.ContainsKey(plantType))
            return null;
        if (externalData.plants[plantType].Keys.Count == 0)
        {
            if (Application.isEditor)
                DispatchQueryResult("{\"data\":{\"getAllPlants\":{\"Fleur\":[{\"node\":{\"name\":\"Abricotier\"}},{\"node\":{\"name\":\"Campanule\"}},{\"node\":{\"name\":\"Capucine\"}},{\"node\":{\"name\":\"Coquelicot\"}},{\"node\":{\"name\":\"Crocus\"}},{\"node\":{\"name\":\"Edelweiss\"}},{\"node\":{\"name\":\"Gardénia\"}},{\"node\":{\"name\":\"Jacinthe\"}},{\"node\":{\"name\":\"Lys\"}},{\"node\":{\"name\":\"Narcisse\"}},{\"node\":{\"name\":\"Œillet\"}},{\"node\":{\"name\":\"Oeillet d'Inde\"}},{\"node\":{\"name\":\"Orchidées\"}},{\"node\":{\"name\":\"Pensée\"}},{\"node\":{\"name\":\"Pétunia\"}},{\"node\":{\"name\":\"Phalaenopsis\"}},{\"node\":{\"name\":\"Pivoine\"}}]}}}");
            else if (externalData.plants[plantType].Keys.Count == 0)
                SendQuery(graphQL.GetPlantsOfType(plantType, externalData.plantsTypes[plantType]));
            return null;
        }
        return externalData.plants[plantType].Values.Select(x => x.name).ToArray();
    }

    public PlantData GetPlantsData(string plantType, string plantName)
    {
        if (!externalData.plants.ContainsKey(plantType) || !externalData.plants[plantType].ContainsKey(plantName))
            return null;
        if (!externalData.plants[plantType][plantName].requested)
        {
            if (Application.isEditor)
                DispatchQueryResult("{\"data\":{\"getPlant\":{\"name\":\"Pétunia\",\"blossomingEnd\":[4],\"blossomingStart\":[2],\"color\":[\"Vert\",\"Rose\"],\"createdAt\":\"Sat, 04 May 2019 23:45:58 +0000\",\"groundTypes\":[{\"id\":\"e060619a-7977-4280-ba21-f5673ebb6817\"},{\"id\":\"e09f04bf-57a0-45cc-8e89-00f3657bd287\"},{\"id\":\"f6ddd7f8-329e-4809-a869-40281392823b\"}],\"heightLow\":10000,\"heightHigh\":null,\"id\":\"6bcc1cc7-32dd-4e38-b516-7b70d341707f\",\"periodicities\":[{\"name\":\"Vivace\"}],\"phRangeLow\":7,\"phRangeHigh\":7,\"photo\":\"https://i.ytimg.com/vi/9L0HzzrE-ck/hqdefault.jpg\",\"rusticity\":5,\"shapes\":[{\"name\":\"Arrondi\"}],\"sunNeed\":8,\"thumbnail\":\"https://i.ytimg.com/vi/9L0HzzrE-ck/hqdefault.jpg\",\"type\":{\"name\":\"Fleur\"},\"updatedAt\":\"Sat, 04 May 2019 23:45:59 +0000\"}}}");
            else
                SendQuery(graphQL.GetPlantData(externalData.plants[plantType][plantName].plantID));
            return null;
        }
        return externalData.plants[plantType][plantName];
    }
}
