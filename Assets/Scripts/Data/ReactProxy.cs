﻿using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class ReactProxy : MonoBehaviour
{
    public static ReactProxy instance = null;

    public GraphQL graphQL;
    public ExternalData externalData;
    public Dictionary<string, Action<string>> callbacks = new Dictionary<string, Action<string>>();

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
            externalData = new ExternalData(callbacks);
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
        /*if (Application.isEditor)
            DispatchQueryResult("{\"data\":{\"getPlant\":{\"type\":{\"name\":\"Fleur\"},\"name\":\"Pétunia\",\"colors\":[{\"name\":\"Rose\"},{\"name\":\"Blanche\"},{\"name\":\"Orange\"},{\"name\":\"Rouge\"},{\"name\":\"Jaune\"},{\"name\":\"Violet\"},{\"name\":\"Bleu\"}],\"phRangeLow\":0,\"phRangeHigh\":7,\"thumbnail\":\"https://s3.greefine.ovh/dev/90c2a47695df1ba2e9063e690639cb2d5cc57e40/thumbnail_3abb3ce3-03ec-4206-b146-7861663ce989.jpg\",\"rusticity\":5,\"sunNeed\":7,\"waterNeed\":9}}}");
        else*/
            query(payload);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Keypad5))
            InitScene("{\"name\":\"Offline Garden\",\"boundaries\":[{\"x\":0,\"y\":0},{\"x\":100,\"y\":100}],\"garden\":[{\"type\":\"FlowerBed\",\"data\":{\"name\":\"Parterre 1\",\"soilType\":\"Argileux\",\"points\":[{\"x\":65.96473693847656,\"y\":-56.58185577392578},{\"x\":57.210060119628906,\"y\":-62.110374450683594},{\"x\":60,\"y\":-64},{\"x\":62.411865234375,\"y\":-63.446311950683594}],\"elements\":[{\"subID\":\"\",\"position\":{\"x\":59.495323181152344,\"y\":0,\"z\":-62.03126525878906},\"rotation\":{\"x\":0,\"y\":0,\"z\":0,\"w\":1}},{\"subID\":\"\",\"position\":{\"x\":61,\"y\":0,\"z\":-63},\"rotation\":{\"x\":0,\"y\":0,\"z\":0,\"w\":1}},{\"subID\":\"\",\"position\":{\"x\":61.25973892211914,\"y\":0,\"z\":-60.508094787597656},\"rotation\":{\"x\":0,\"y\":0,\"z\":0,\"w\":1}},{\"subID\":\"\",\"position\":{\"x\":62.30310821533203,\"y\":0,\"z\":-61.42021560668945},\"rotation\":{\"x\":0,\"y\":0,\"z\":0,\"w\":1}},{\"subID\":\"\",\"position\":{\"x\":63.37784957885742,\"y\":0,\"z\":-59.27602767944336},\"rotation\":{\"x\":0,\"y\":0,\"z\":0,\"w\":1}}]}}]}");
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
        if (!externalData.plants.ContainsKey(plantType)
            || !externalData.plants[plantType].ContainsKey(plantName)
            || externalData.plants[plantType][plantName].status == PlantData.DataStatus.Requested)
            return null;
        if (externalData.plants[plantType][plantName].status == PlantData.DataStatus.None)
        {
            Debug.Log(plantName + " Status None");
            if (Application.isEditor)
            {
                AsyncFaker();
                DispatchQueryResult("{\"data\":{\"getPlant\":{\"type\":{\"name\":\"Fleur\"},\"name\":\"Pétunia\",\"colors\":[{\"name\":\"Rose\"},{\"name\":\"Blanche\"},{\"name\":\"Orange\"},{\"name\":\"Rouge\"},{\"name\":\"Jaune\"},{\"name\":\"Violet\"},{\"name\":\"Bleu\"}],\"phRangeLow\":0,\"phRangeHigh\":7,\"thumbnail\":\"https://s3.greefine.ovh/dev/90c2a47695df1ba2e9063e690639cb2d5cc57e40/thumbnail_3abb3ce3-03ec-4206-b146-7861663ce989.jpg\",\"rusticity\":5,\"sunNeed\":7,\"waterNeed\":9}}}");
            }
            else
                SendQuery(graphQL.GetPlantData(externalData.plants[plantType][plantName].plantID));
            externalData.plants[plantType][plantName].status = PlantData.DataStatus.Requested;
            return null;
        }
        return externalData.plants[plantType][plantName];
    }

    public void LoadPlantDataFromId(string plantID, Action<PlantData> callback)
    {
        SendQuery(graphQL.GetPlantData(plantID));
    }

    public PlantData.DataStatus GetPlantStatus(string plantName, string plantType)
    {
        return externalData.plants[plantType][plantName].status;
    }

    public void SetPlantStatus(string plantName, string plantType, PlantData.DataStatus status)
    {
        externalData.plants[plantType][plantName].status = status;
    }

    private IEnumerator AsyncFaker()
    {
        yield return new WaitForSeconds(2.0f);
    }
}