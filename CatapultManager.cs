using System;
using UnityEditor;
using HarmonyLib;

[HaromnyPatch(typeof(CarrierCatapult), nameof(CarrierCatapult.Awake))]
public class ExtendCatapultLaunch
{
	[HaromnyPrefix]
	//public static void Prefix(ref float ___launchTime)
	public static void Prefix(CarrierCatapult __instance)
	{
		//___launchTime = 7;
		Traverse traverse = Traverse.create(__instance);
	}
}
