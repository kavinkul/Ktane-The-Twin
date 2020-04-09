﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Rnd = UnityEngine.Random;
using KModkit;

public class TheTwinScript : MonoBehaviour
{
	public KMAudio Audio;
	public KMBombModule Module;
	public KMBombInfo Info;
	public KMBossModule BossHandler;
	public GameObject ModuleObject;
	public GameObject StatusLight;
	public GameObject ScreensAndButtons;
	public GameObject[] ButtonObjects;
	public TextMesh StageDisplay;
	public TextMesh ModulePairIdDisplay;
	public Renderer ModuleBackground;
	public Material[] BackgroundColor;

	sealed class TheTwinBombInfo
    {
		public List<TheTwinScript> Modules = new List<TheTwinScript>();
    };

	private static readonly Dictionary<string,TheTwinBombInfo> _TheTwinInfos = new Dictionary<string, TheTwinBombInfo>();
	private TheTwinBombInfo _TheTwinInfo;
	private TheTwinScript _modulePair = null;
	private bool _isReady = false;
	private bool[] _isPressed;
	private int _startingNumber;
	private int _sequenceLength = 0;
	private int _totalModules;
	private string _finalSequence = "";
	private string _removeSet = "";
	private string[] _ignoredModules;
	private readonly string[][] _removeGrid = new string[10][] { new string[12] { "371", "816", "138", "594", "293", "074", "675", "463", "319", "572", "503", "413" },
                                                                 new string[12] { "218", "627", "236", "941", "517", "037", "437", "804", "620", "014", "980", "135" },
                                                                 new string[12] { "438", "652", "694", "482", "569", "096", "985", "608", "521", "637", "179", "539" },
                                                                 new string[12] { "570", "601", "524", "853", "019", "349", "897", "803", "875", "028", "574", "514" },
                                                                 new string[12] { "706", "298", "043", "320", "783", "603", "180", "079", "592", "290", "124", "871" },
                                                                 new string[12] { "258", "354", "487", "962", "456", "723", "518", "148", "782", "894", "417", "129" },
                                                                 new string[12] { "021", "123", "638", "105", "146", "342", "573", "402", "615", "701", "940", "973" },
                                                                 new string[12] { "642", "013", "523", "846", "059", "942", "961", "938", "271", "548", "382", "591" },
                                                                 new string[12] { "093", "085", "896", "796", "746", "653", "396", "316", "198", "780", "268", "406" },
                                                                 new string[12] { "794", "568", "927", "650", "716", "054", "502", "270", "427", "216", "795", "867" } };
	private readonly int[][] _colorGrid = new int[5][] { new int[6] { 2, 2, 6, 6, 5, 7 },
	                                                     new int[6] { 3, 3, 1, 1, 5, 7 },
	                                                     new int[6] { 1, 5, 4, 7, 6, 6 },
	                                                     new int[6] { 1, 5, 4, 7, 3, 3 },
	                                                     new int[6] { 4, 7, 3, 2, 2, 4 } };
	private int[] _startingCoordinate = new int[2] { 0, 0 };
	private int[] _currentCoordinate = new int[2] { 0, 0 };
	private int[] _startingColorGridCoordinate = new int[2] { 0, 0 };
	private int[] _currentColorGridCoordinate = new int[2] { 0, 0 };
	private int _currentDigit = 0;
	private int _currentStage = 0;
	private int _direction = 0;
	private List<int> _stageColor = new List<int>();
	private List<string> _removeSetList = new List<string>();
	private List<bool> _changeRemoveSet = new List<bool>();
	private Coroutine ActiveCoroutine;
	private bool _isActivated = false;
	private bool _autoSolved = false;
	private bool _cycleCompleted = true;
	private int _modulePairId;
	private bool _isInitialized = false;
	private bool _isGenerated = false;
	private bool _assignedRNG = false;
	private int _swapCase;
	private bool _finishedTrading = false;
	private readonly string[] _colorNames = new string[7] { "Red", "Green", "Blue", "Yellow", "White", "Purple", "Emerald" };
	
	//Logging
	static int moduleIdCounter = 1;
	private int _moduleId;
	private bool _moduleSolved = false;
	
	void Awake()
    {
		_moduleId = moduleIdCounter++;
		_isPressed = new bool[ButtonObjects.Length];
	}

	void Start () 
	{
		if (_ignoredModules == null)
			_ignoredModules = BossHandler.GetIgnoredModules("The Twin", new string[]{
				"14",
				"Cruel Purgatory",
				"Forget Enigma",
				"Forget Everything",
				"Forget It Not",
				"Forget Me Later",
				"Forget Me Not",
				"Forget Perspective",
				"Forget The Color",
				"Forget Them All",
				"Forget This",
				"Forget Us Not",
				"Organization",
				"Purgatory",
				"RPS Judging",
				"Simon's Stages",
				"Souvenir",
				"Tallordered Keys",
				"The Time Keeper",
				"Timing is Everything",
				"The Troll",
				"The Twin",
				"Turn The Key",
				"Übermodule",
				"Ültimate Custom Night",
				"The Very Annoying Button"
			});
		for (int index = 0; index < ButtonObjects.Length; index++)
        {
			int j = index;
			_isPressed[j] = false;
			ButtonObjects[index].GetComponent<KMSelectable>().OnInteract += delegate () { PressButton(j); return false; };
		}

		var serialNumber = Info.GetSerialNumber();
		if (!_TheTwinInfos.ContainsKey(serialNumber))
			_TheTwinInfos[serialNumber] = new TheTwinBombInfo();
		_TheTwinInfo = _TheTwinInfos[serialNumber];
		_TheTwinInfo.Modules.Add(this);
		_modulePairId = (_TheTwinInfo.Modules.Count() + 1)/ 2;
		Debug.LogFormat("[The Twin #{0}] The pair ID is {1}.", _moduleId, _modulePairId);
		if (_TheTwinInfo.Modules.Count() % 2 == 0)
		{
			ModuleObject.transform.localScale = new Vector3(-1f, 1f, 1f);
			ScreensAndButtons.transform.localScale = new Vector3(-1f, 1f, 1f);
			Vector3 position = StatusLight.transform.localPosition;
			position.x *= -1;
			StatusLight.transform.localPosition = position;
		}
		UpdatePairIdScreen(_modulePairId);

		_startingNumber = Rnd.Range(0, 100);
		Debug.LogFormat("[The Twin #{0}] The initial number is {1}.", _moduleId, _startingNumber);
		UpdateStageScreen(_startingNumber);
		_startingCoordinate[0] = Rnd.Range(0, 12);
		_startingCoordinate[1] = Rnd.Range(0, 10);
		Debug.LogFormat("[The Twin #{0}] The starting coordinate in remove table is ({1}, {2}).", _moduleId, _startingCoordinate[0], _startingCoordinate[1]);
		_currentCoordinate[0] = _startingCoordinate[0];
		_currentCoordinate[1] = _startingCoordinate[1];
		_removeSet = _removeGrid[_startingCoordinate[1]][_startingCoordinate[0]];
		_startingColorGridCoordinate[0] = _startingCoordinate[0] % 6;
		_startingColorGridCoordinate[1] = _startingCoordinate[1] % 5;
		Debug.LogFormat("[The Twin #{0}] The starting coordinate in color table is ({1}, {2}).", _moduleId, _startingColorGridCoordinate[0], _startingColorGridCoordinate[1]);
		_currentColorGridCoordinate[0] = _startingColorGridCoordinate[0];
		_currentColorGridCoordinate[1] = _startingColorGridCoordinate[1];

		Module.OnActivate += delegate ()
		{
			_totalModules = Info.GetSolvableModuleNames().Where(a => !_ignoredModules.Contains(a)).ToList().Count;
			Debug.LogFormat("[The Twin #{0}] There are {1} non-ignored modules.", _moduleId, _totalModules);
			if (_totalModules < 2)
            {
				Debug.LogFormat("[The Twin #{0}] Too few non-ignored modules. Solving the module.", _moduleId);
				_autoSolved = true;
				_moduleSolved = true;
            }
			else
            {
				_isActivated = true;
				_sequenceLength = 3 * (_totalModules - 1);
				Debug.LogFormat("[The Twin #{0}] The initial remove set is {1}.", _moduleId, _removeSet);
				Generate();
			}
		};
	}

	// Update is called once per frame
	void Update () 
	{
		if (_autoSolved)
		{
			Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
			Module.HandlePass();
			_isReady = true;
			_moduleSolved = true;
			_autoSolved = false;
		}
		if (_moduleSolved || !_isActivated || !_cycleCompleted || _isReady) return;
		int solveCount = Info.GetSolvedModuleNames().Where(a => !_ignoredModules.Contains(a)).ToList().Count;
		if (_totalModules == solveCount && _currentStage >= solveCount)
		{
			if (ActiveCoroutine != null)
				StopCoroutine(ActiveCoroutine);
			_isReady = true;
			UpdateStageScreen("--");
			ModuleBackground.material = BackgroundColor[0];
			StageDisplay.color = Color.white;
			return;
		}
		if (_currentStage < solveCount)
		{
			_currentStage++;
			if (_currentStage == _totalModules) return;
			_cycleCompleted = false;
			UpdateStageScreen(_currentStage);
			if (ActiveCoroutine != null)
				StopCoroutine(ActiveCoroutine);
			ActiveCoroutine = StartCoroutine(CycleColor());
        }
        else if (solveCount == 0)
		{
			if (ActiveCoroutine == null)
				ActiveCoroutine = StartCoroutine(DisplayCode());
		}
	}

	private void PressButton(int index)
    {
		if (_isPressed[index]) return;
		ButtonObjects[index].GetComponent<KMSelectable>().AddInteractionPunch(0.125f);
		StartCoroutine(AnimateButton(index));
		if (!_isReady)
        {
			Debug.LogFormat("[The Twin #{0}] Module is not ready to be solved. Initiating a strike.", _moduleId);
			Module.HandleStrike();
			return;
        }
		if (_modulePair != null)
        {
			if (_modulePair._moduleSolved) return;
			if (index.ToString()[0] == _modulePair._finalSequence[_modulePair._currentDigit])
			{
				_modulePair.UpdateSubmissionDisplay();
				_modulePair._currentDigit++;
				if (_modulePair._currentDigit == _modulePair._finalSequence.Length)
				{
					Debug.LogFormat("[The Twin #{0}] All digits entered are correct! Solving The Twin #{1}.", _moduleId, _modulePair._moduleId);
					_modulePair.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
					_modulePair.Module.HandlePass();
					_modulePair._moduleSolved = true;
				}
			}
			else
			{
				Debug.LogFormat("[The Twin #{0}] The {1} digit of The Twin #{2} is {3}. Entered {4}. Not correct. Initiating a strike on The Twin #{5}.", _moduleId, _modulePair._moduleId, Ordinal(_modulePair._currentDigit + 1), _modulePair._finalSequence[_modulePair._currentDigit], index, _modulePair._moduleId);
				_modulePair.Module.HandleStrike();
			}
		}
		else
        {
			if (_moduleSolved) return;
			if (index.ToString()[0] == _finalSequence[_currentDigit])
			{
				UpdateSubmissionDisplay();
				_currentDigit++;
				if (_currentDigit == _finalSequence.Length)
				{
					Debug.LogFormat("[The Twin #{0}] All digits entered are correct! Solving the module.", _moduleId);
					Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
					Module.HandlePass();
					_moduleSolved = true;
				}
			}
			else
			{
				Debug.LogFormat("[The Twin #{0}] The {1} digit is {2}. Entered {3}. Not correct. Initiating a strike.", _moduleId, Ordinal(_currentDigit + 1), _finalSequence[_currentDigit], index);
				Module.HandleStrike();
			}
		}
    }

	private string Ordinal(int index)
    {
		if (index % 100 < 10 || index % 100 > 19)
        {
			if (index % 10 == 1)
				return index.ToString() + "st";
			else if (index % 10 == 2)
				return index.ToString() + "nd";
			else if (index % 10 == 3)
				return index.ToString() + "rd";
        }
		return index.ToString() + "th";
	}

	private IEnumerator AnimateButton(int index)
    {
		_isPressed[index] = true;
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		ButtonObjects[index].GetComponent<KMSelectable>().Highlight.transform.localPosition -= new Vector3(0, .05f, 0); 
		for (int count = 0; count < 6; count++)
        {
			ButtonObjects[index].transform.localPosition -= new Vector3(0,.00067f,0);
			yield return new WaitForSeconds(.005f);
        }
		for (int count = 0; count < 6; count++)
		{
			ButtonObjects[index].transform.localPosition += new Vector3(0, .00067f, 0);
			yield return new WaitForSeconds(.005f);
		}
		ButtonObjects[index].GetComponent<KMSelectable>().Highlight.transform.localPosition += new Vector3(0, .05f, 0);
		_isPressed[index] = false;
	}

	private void GenerateRemoveSets()
    {
		bool[] changeRemoveSet = { false, false, false, false, false, false, true };
		changeRemoveSet = changeRemoveSet.Shuffle();
		int tries = 0;
		_removeSetList.Add(_removeSet);
		for (int count = 0; count < _sequenceLength; count++)
		{
			_direction = Rnd.Range(0, 4);
			NextPositionInGrids(_direction);
			_changeRemoveSet.Add(changeRemoveSet[tries]);

			//If this is the last digit, then there is no reason to reassign the remove set.
			if (count == _sequenceLength - 1)
            {
				_changeRemoveSet[count] = false;
				changeRemoveSet[tries] = false;
            }
			if (changeRemoveSet[tries])
			{
				_removeSetList.Add(_removeGrid[_currentCoordinate[1]][_currentCoordinate[0]]);
				changeRemoveSet = changeRemoveSet.Shuffle();
				tries = 0;
			}
			else
				tries++;
		}

		string colorLogging = " | ";
		for (int index = 0; index < _stageColor.Count(); index++)
			colorLogging += (_colorNames[_stageColor[index] - 1] + " | ");
		Debug.LogFormat("[The Twin #{0}] The list of background colors in the sequence phase is [{1}]", _moduleId, colorLogging);
	}

	private void Generate()
	{
		GenerateRemoveSets();

		_isInitialized = true;

		var pairedModule = _TheTwinInfo.Modules.Where(module => module._modulePairId == _modulePairId && module._moduleId != _moduleId);
		if (pairedModule.Count() == 1)
        {
			Debug.LogFormat("[The Twin #{0}] The module found a pair. Trading information...", _moduleId);
			StartCoroutine(TradeInfo(pairedModule));
		}
		else
        {
			Debug.LogFormat("[The Twin #{0}] The module did not find a pair. Generating final string.", _moduleId);
			StartCoroutine(GenerateFinalString());
		}
	}

	private IEnumerator TradeInfo(IEnumerable<TheTwinScript> module)
	{
		_modulePair = module.ElementAt(0);
		if (_moduleId < _modulePair._moduleId)
			_swapCase = Rnd.Range(0, 4);
		else
        {
			while (!_modulePair._assignedRNG)
				yield return new WaitForSeconds(.1f);
			_swapCase = module.ElementAt(0)._swapCase;
		}
		_assignedRNG = true;
		while (!_modulePair._isInitialized)
			yield return new WaitForSeconds(.1f);
		switch (_swapCase)
        {
			case 0: //Manipulating digits in final sequences
				ModulePairIdDisplay.color = Color.red;
				Debug.LogFormat("[The Twin #{0}] The ID display is red. Manipulating digits in final sequences.", _moduleId);
				yield return StartCoroutine(GenerateFinalString());
				while (!_modulePair._isGenerated)
					yield return new WaitForSeconds(.1f);
				int finalSequenceLength = 1;
				if (_finalSequence.Length < _modulePair._finalSequence.Length)
					finalSequenceLength = _finalSequence.Length;
				else
					finalSequenceLength = _modulePair._finalSequence.Length;
				StringBuilder finalSequence = new StringBuilder(_finalSequence.Substring(0, finalSequenceLength));
				StringBuilder otherFinalSequence = new StringBuilder(_modulePair._finalSequence.Substring(0, finalSequenceLength));
				_finishedTrading = true;
				while (!_modulePair._finishedTrading)
					yield return new WaitForSeconds(.1f);
				if (_moduleId < _modulePair._moduleId)
					for (int index = 0; index < finalSequenceLength; index++)
                    {
						if (finalSequence[index] > otherFinalSequence[index])
						{
							finalSequence[index] = otherFinalSequence[index];
							_finalSequence = finalSequence.ToString() ;
						}
                    }
                else
					for (int index = 0; index < finalSequenceLength; index++)
					{
						if (finalSequence[index] < otherFinalSequence[index])
						{
							finalSequence[index] = otherFinalSequence[index];
							_finalSequence = finalSequence.ToString();
						}
					}
				Debug.LogFormat("[The Twin #{0}] The final sequence is now {1}.", _moduleId, _finalSequence);
				break;
			case 1: //Trading the initial number
				ModulePairIdDisplay.color = Color.green;
				Debug.LogFormat("[The Twin #{0}] The ID display is green. Trading the starting number.", _moduleId);
				var startingNumber = _modulePair._startingNumber;
				_finishedTrading = true;
				while (!_modulePair._finishedTrading)
					yield return new WaitForSeconds(.1f);
				_startingNumber = startingNumber;
				Debug.LogFormat("[The Twin #{0}] The starting number is now {1}.", _moduleId, _startingNumber);
				yield return StartCoroutine(GenerateFinalString());
				break;
			case 2: //Trading background colors
				ModulePairIdDisplay.color = Color.blue;
				Debug.LogFormat("[The Twin #{0}] The ID display is blue. Trading the background colors after stage 0.", _moduleId);
				yield return StartCoroutine(GenerateFinalString());
				var colorList = _modulePair._stageColor;
				_finishedTrading = true;
				while (!_modulePair._finishedTrading)
					yield return new WaitForSeconds(.1f);
				_stageColor = new List<int>(colorList);
				string colorLogging = " | ";
				for (int index = 0; index < _stageColor.Count(); index++)
					colorLogging += (_colorNames[_stageColor[index] - 1] + " | ");
				Debug.LogFormat("[The Twin #{0}] The list of background colors in the sequence phase is now [{1}]", _moduleId, colorLogging);
				break;
			case 3: //Trading the remove set
				ModulePairIdDisplay.color = Color.yellow;
				Debug.LogFormat("[The Twin #{0}] The ID display is yellow. Trading the remove sets.", _moduleId);
				var removeList = _modulePair._removeSetList;
				var changeRemoveSet = _modulePair._changeRemoveSet;
				_finishedTrading = true;
				while (!_modulePair._finishedTrading)
					yield return new WaitForSeconds(.1f);
				_removeSetList = new List<string>(removeList);
				_changeRemoveSet = new List<bool>(changeRemoveSet);
				_removeSet = _removeSetList[0];
				Debug.LogFormat("[The Twin #{0}] The new initial remove set is {1}.", _moduleId, _removeSet);
				yield return StartCoroutine(GenerateFinalString());
				break;
        }
	}

	private IEnumerator GenerateFinalString()
	{
		int removeSetIndex = 0;
		for (int count = 0; count < _sequenceLength; count++)
		{
			_finalSequence += new string((_startingNumber + count + 1).ToString().Where(ch => !_removeSet.Contains(ch)).ToArray());
			if (_changeRemoveSet[count])
			{
				removeSetIndex++;
				_removeSet = _removeSetList[removeSetIndex];
				Debug.LogFormat("[The Twin #{0}] After the {1} number, the new remove set for subsequence numbers is: {2}", _moduleId, Ordinal(count + 1), _removeSet);
			}
		}
		//If the string managed to be empty somehow, default to "0".
		if (_finalSequence == "")
			_finalSequence = "0";

		_isGenerated = true;

		Debug.LogFormat("[The Twin #{0}] The final sequence is {1}.", _moduleId, _finalSequence);

		yield return null;
	}

	private void NextPositionInGrids(int direction)
    {
		switch (direction)
        {
			case 0: //Move Up
				if (_currentCoordinate[1] == 0)
					_currentCoordinate[1] = 9;
				else
					_currentCoordinate[1]--;

				if (_currentColorGridCoordinate[1] == 0)
					_currentColorGridCoordinate[1] = 4;
				else
					_currentColorGridCoordinate[1]--;
				break;
			case 1: //Move Right
				if (_currentCoordinate[0] == 11)
					_currentCoordinate[0] = 0;
				else
					_currentCoordinate[0]++;

				if (_currentColorGridCoordinate[0] == 5)
					_currentColorGridCoordinate[0] = 0;
				else
					_currentColorGridCoordinate[0]++;
				break;
			case 2: //Move Down
				if (_currentCoordinate[1] == 9)
					_currentCoordinate[1] = 0;
				else
					_currentCoordinate[1]++;

				if (_currentColorGridCoordinate[1] == 4)
					_currentColorGridCoordinate[1] = 0;
				else
					_currentColorGridCoordinate[1]++;
				break;
			case 3: //Move Left
				if (_currentCoordinate[0] == 0)
					_currentCoordinate[0] = 11;
				else
					_currentCoordinate[0]--;

				if (_currentColorGridCoordinate[0] == 0)
					_currentColorGridCoordinate[0] = 5;
				else
					_currentColorGridCoordinate[0]--;
				break;
        }
		_stageColor.Add(_colorGrid[_currentColorGridCoordinate[1]][_currentColorGridCoordinate[0]]);
	}
	
	private IEnumerator CycleColor()
    {
		int backgroundCycleStep = 0;
		while (true)
        {
			int[] color = new int[4] { 0, _stageColor[3 * (_currentStage - 1)], _stageColor[3 * (_currentStage - 1) + 1], _stageColor[3 * (_currentStage - 1) + 2] };
			bool[] textIsRed = new bool[4] { false, _changeRemoveSet[3 * (_currentStage - 1)], _changeRemoveSet[3 * (_currentStage - 1) + 1], _changeRemoveSet[3 * (_currentStage - 1) + 2] };
			ModuleBackground.material = BackgroundColor[color[backgroundCycleStep]];
			StageDisplay.color = textIsRed[backgroundCycleStep] ? Color.red : Color.white;
			backgroundCycleStep = (backgroundCycleStep + 1) % 4;
			yield return new WaitForSeconds(1);
			if (backgroundCycleStep == 0)
				_cycleCompleted = true;
		}
	}

	private IEnumerator DisplayCode()
    {
		int[][] colorOrder = new int[12][] { new int[5] { 1, 6, 5, 4, 3 },
			                                 new int[5] { 2, 7, 5, 1, 6 },
			                                 new int[5] { 4, 2, 7, 6, 1 },
			                                 new int[5] { 1, 2, 3, 4, 5 },
			                                 new int[5] { 4, 2, 1, 6, 5 },
			                                 new int[5] { 2, 1, 4, 6, 7 },
			                                 new int[5] { 1, 6, 2, 3, 4 },
			                                 new int[5] { 7, 1, 5, 2, 4 },
			                                 new int[5] { 2, 7, 3, 5, 1 },
			                                 new int[5] { 4, 6, 5, 1, 7 },
		                                     new int[5] { 6, 7, 1, 4, 2 },
		                                     new int[5] { 5, 1, 7, 3, 6 } };
		string[] morseCode = new string[10] { "-----", ".----", "..---", "...--", "....-", ".....", "-....", "--...", "---..", "----." };
		float dotLength = .27f;
		ModuleBackground.material = BackgroundColor[0];
		yield return new WaitForSeconds(dotLength * 3);
		while (true)
		{
			for (int step = 0; step < 5; step++)
            {
				ModuleBackground.material = BackgroundColor[colorOrder[_startingCoordinate[0]][step]];
				if (morseCode[_startingCoordinate[1]][step] == '-')
					yield return new WaitForSeconds(dotLength * 3);
				else
					yield return new WaitForSeconds(dotLength);
				ModuleBackground.material = BackgroundColor[0];
				yield return new WaitForSeconds(dotLength);
			}
			yield return new WaitForSeconds(dotLength * 2);
        }
    }

	private void UpdateStageScreen(int number)
    {
		int num = number % 100;
		if (num < 10)
			StageDisplay.text = "0" + num.ToString();
		else
			StageDisplay.text = num.ToString();
	}

	private void UpdateStageScreen(string text)
    {
		if (text.Length > 2)
			StageDisplay.text = text.Substring(0, 2);
		else
			StageDisplay.text = text;
    }

	private void UpdatePairIdScreen(int number)
	{
		int num = number % 100;
		if (num < 10)
			ModulePairIdDisplay.text = "0" + num.ToString();
		else
			ModulePairIdDisplay.text = num.ToString();
	}

	private void UpdateSubmissionDisplay()
	{
		Debug.LogFormat("{0}", _currentDigit);
		if (_currentDigit == _finalSequence.Length) return;
		if (_currentDigit == 0)
			UpdateStageScreen("-" + _finalSequence[0].ToString());
		else
			UpdateStageScreen(_finalSequence.Substring(_currentDigit - 1, 2));
	}
}
