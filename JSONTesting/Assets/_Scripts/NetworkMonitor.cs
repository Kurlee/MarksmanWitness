﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Net;
using System.Text;
/// <summary>
/// Author: Ben Hoffman
/// This class will be the main controller for the network monitoring in this visualization.
/// It will send HTTP GET requests to my ELK server, and use Unity's JsonUtility to 
/// put the data into a data object in C#. Then that data will be analyzed to see 
/// IF that IP address has been seen here before, and if has, then we will tell our computer
/// controller to make a new computer with the given information. 
/// </summary>
public class NetworkMonitor : MonoBehaviour {

    #region Fields
    // The URL of my ELK server
    private string elk_url = "http://192.168.137.134:9200/packetbeat-2017.01.27/_search?pretty=true";
    private string JSON_String = "";        // The string that represents the JSON data
    private Json_Data dataObject;           // The actual JSON data class 

    private string queryLocation;
    private string queryString;
    private GameController gameControllerObj; // The game controller 

    // These are fields for my web request so that I am not making new objects in mem
    // With each call
    private WebRequest request;
    private HttpWebResponse response;
    private Stream requestStream;
    private StreamReader reader;
    private byte[] buffer;
    private string bufferResult;
    #endregion

    /// <summary>
    /// Author: Ben Hoffman
    /// Purpose: Just start the coroutine that will constantly find the data
    /// </summary>
    void Start()
    {
        queryLocation =  Application.streamingAssetsPath + "/gimmeData.json";
        queryString = File.ReadAllText(queryLocation);

        gameControllerObj = GameObject.FindObjectOfType<GameController>();

        // Find the latest index name and make my URL, or maybe get all the indexes and ask the
        // user which one they want to use

        StartCoroutine(SetJsonData());
    }


    /// <summary>
    /// Author: Ben Hoffman
    /// This will allow me to get the most recent event
    /// from the ELK server, in a string that can then be parsed 
    /// by the Unity JsonUtility. This is a co routine so that
    /// I don't get framerate drops if it takes too long
    /// to download
    /// </summary>
    IEnumerator SetJsonData()
    {
        // Make a new web request with the .net WebRequest
        request = WebRequest.Create(elk_url);

        request.ContentType = "application/json";
        request.Method = "POST";
        buffer = Encoding.GetEncoding("UTF-8").GetBytes(queryString);
        bufferResult = System.Convert.ToBase64String(buffer);

        requestStream = request.GetRequestStream();
        requestStream.Write(buffer, 0, buffer.Length);
        requestStream.Close();

        response = (HttpWebResponse)request.GetResponse();

        yield return requestStream = response.GetResponseStream();

        // Open the stream using a StreamReader for easy access.
        reader = new StreamReader(requestStream);
        // Set my string to the response from the website
        JSON_String = reader.ReadToEnd();

        // Cleanup the streams and the response.
        reader.Close();
        requestStream.Close();
        response.Close();

        // As long as we are not null, put this in as real C# data
        if(JSON_String != "" || JSON_String != null)
        {
            StringToJson();
        }

        // Capture data every certain number of seconds
        //yield return new WaitForSeconds(1f);

        // Start this again after 1 second
        StartCoroutine(SetJsonData());
    }

    /// <summary>
    /// Author: Ben Hoffman
    /// Take the Json string that we just got from the website,
    /// and use the JsonUtility to make it JsonData. After that 
    /// it will send it to the game controller
    /// </summary>
    public void StringToJson()
    {
        // Use the JsonUtility to send the string of data that I got from the server, to a data object
        dataObject = JsonUtility.FromJson<Json_Data>(JSON_String);

        // As long as we did not time out... 
        if (!dataObject.timed_out)
        {
            // Give my game controller the JSON data to sort out if there is a new computer on it or not
            gameControllerObj.CheckIP(dataObject);
        }
    }


}