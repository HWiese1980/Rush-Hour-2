﻿using ColossalFramework.HTTP;
using RushHour2.Core.Reporting;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

namespace RushHour2.Localisation.Language
{
    public static class GoogleTranslate
    {
        public static string GetTranslationFor(string text, string toIsoCode, string fromIsoCode = "en-gb")
        {
            LoggingWrapper.Log(LoggingWrapper.LogArea.File, LoggingWrapper.LogType.Message, $"Translating {text} into {toIsoCode} from {fromIsoCode}...");

            try
            {
                var safeText = WWW.EscapeURL(text);
                var request = new Request("get", $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={fromIsoCode}&tl={toIsoCode}&dt=t&q={safeText}");
                request.AddHeader("Accept-Language", "en");
                request.acceptGzip = false;

                request.Send();

                var timeout = DateTime.Now + TimeSpan.FromSeconds(5);

                while (!request.isDone || DateTime.Now > timeout)
                {
                    Thread.Sleep(10);
                }

                if (request.response != null)
                {
                    return GetTranslation(request.response);
                }
            }
            catch (Exception ex)
            {
                LoggingWrapper.Log(LoggingWrapper.LogArea.File, LoggingWrapper.LogType.Error, "There was an issue trying to get an automatic translation from Google Translate");
                LoggingWrapper.Log(LoggingWrapper.LogArea.File, ex);
            }

            return null;
        }

        private static string GetTranslation(Response response)
        {
            if (response != null)
            {
                var jsonString = response.Text;
                if (jsonString != null)
                {
                    var jsonNode = SimpleJSON.JSON.Parse(jsonString);
                    if (jsonNode != null && jsonNode.Count > 0)
                    {
                        var translationGroups = jsonNode[0];
                        if (translationGroups != null && translationGroups.Count > 0)
                        {
                            var fullString = "";

                            foreach (var translation in translationGroups.Children)
                            {
                                if (translation != null && translation.Count > 1)
                                {
                                    var translationText = translation[0];
                                    if (translationText != null)
                                    {
                                        fullString += Regex.Unescape(translationText.Value);
                                    }
                                }
                            }

                            return fullString + "*";
                        }
                    }
                }
            }

            return null;
        }
    }
}
