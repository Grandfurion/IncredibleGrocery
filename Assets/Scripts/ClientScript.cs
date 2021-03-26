using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;


public class ClientScript : MonoBehaviour
{

    public GameObject buyer;
    public GameObject buyerCloud;
    public GameObject cashierCloud;
    public GameObject storage;
    public Button buttonSell;
    public Button buttonSettigs;
    public Button settingsSoundButt;
    public Button settingsMusicButt;
    public Button settingsSaveButt;
    public GameObject settingsWindow;
    public Text textMoney;
    public float aspectRatio;

    public AudioSource musicLoop;
    public AudioSource soundBubbleAppear;
    public AudioSource soundBubbleDisappear;
    public AudioSource musicButtonClick;
    public AudioSource musicMoney;
    public AudioSource musicProductSelect;

    Color transpButton = new Color(1, 1, 1, 0.5f);
    Color transpProduct = new Color(1, 1, 1, 0.3f);
    Color standartColor = new Color(1, 1, 1, 1);
    int orderSize;
    int selectedSize;
    int orderCorrect;
    string saveFilePath;
    FileStream file;
    public SavedData data;

    bool readyToSell = false;

    public Button[] products = new Button[20];

    public GameObject[] order = new GameObject[3];
    int[] orderId = new int[3];
    int[] selectedId = new int[3];


    void Start()
    {
        Camera.main.aspect = aspectRatio;

        saveFilePath = Application.persistentDataPath + "/save";
        dataLoad();
        MoneySet();

        SetClickListeners();
        
            StartCoroutine(NewClient());
        

    }

    void Update()
    {
        
    }

    IEnumerator NewClient()
    {
        buyer.SetActive(true);
       // buyerCloud.SetActive(false);
        buyer.GetComponent<Animator>().SetTrigger("goToCashier");

        yield return new WaitForSeconds(3.5f); 
        GenerateOrder(); // DRAWING CLOUD WITH ORDER
        yield return new WaitForSeconds(5);
        buyerCloud.SetActive(false);
        //PLAY DISAPPEAR SOUND
        if (data.setSound) soundBubbleDisappear.Play();
        buttonSell.GetComponent<Image>().color = transpButton;
        storage.GetComponent<Animator>().SetTrigger("StorageAppear");

    }

    void GenerateOrder()

    {
        //order[0].GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Products/Product3");
        orderSize = Random.Range(0, 3)+1;
        selectedSize = 0;

        buyerCloud.SetActive(true);

        //SOUND CLOUD APPEAR
        if(data.setSound)soundBubbleAppear.Play();

        for(int i = 0; i < orderSize; i++)
        {
            for(;;)
            {
                //next = Random.Range(0, 20)+1;
                orderId[i] = Random.Range(0, 20);
                selectedId[i] = -1;
                if (i == 0 || orderId[i] != orderId[i-1])
                {
                    break;
                }
            }
            order[i].SetActive(true);
        order[i].GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Products/Product"+(orderId[i]+1));
        }
    }

    bool IncludedIn(int[] arr, int val)
    {
        for(int i = 0; i < 3; i++)
        {
            if (arr[i] == val) return true;
        }
        return false;
    }
    void SelectProduct(int n)
    {
        //PLAYING SELECT SOUND
        if (data.setSound) musicProductSelect.Play();
           

        if (IncludedIn(selectedId, n) == false && selectedSize < orderSize)
        {


            selectedId[selectedSize] = n;
            selectedSize++;
            products[n].GetComponent<Image>().color = transpProduct;
            products[n].transform.GetChild(0).gameObject.SetActive(true);
            //SELL BUTTON MODIFICATION
            if (selectedSize == orderSize)
            {
                buttonSell.GetComponent<Image>().color = standartColor;
                readyToSell = true;
            }
        }
        else if(IncludedIn(selectedId, n) == true)
        {
            if (readyToSell)
            {
                readyToSell = false;
                buttonSell.GetComponent<Image>().color = transpButton;
            }
            for(int j = 0; j < 3; j++)
            {
                if(j == 2)
                {
                    selectedId[j] = -1;
                    break;
                }
                if(selectedId[j] == n)
                {
                    selectedId[j] = selectedId[j + 1];
                    selectedId[j + 1] = -1;
                }
                if(selectedId[j] == -1&& j!=2)
                {
                    selectedId[j] = selectedId[j+1];
                }
            }
            selectedSize--;
            products[n].GetComponent<Image>().color = standartColor;
            products[n].transform.GetChild(0).gameObject.SetActive(false);
        }
       
    }

    void OrderProcessing()
    {
        StartCoroutine(OrderProcessingCor());
    }

    IEnumerator OrderProcessingCor()
    {
        if (readyToSell)
        {
            //PLAY BUTTON SOUND
            if (data.setSound) musicButtonClick.Play();

            storage.GetComponent<Animator>().SetTrigger("StorageDisappear");

            cashierCloud.SetActive(true);
            GameObject cashProd;
            for(int j = 0; j < orderSize; j++)
            {
                cashProd = cashierCloud.transform.GetChild(j).gameObject;
                cashProd.SetActive(true);
                cashProd.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Products/Product" + (selectedId[j] + 1));

            }

            yield return new WaitForSeconds(1);

            for (int i = 0; i < orderSize; i++)
            {
                cashProd = cashierCloud.transform.GetChild(i).gameObject;
                if (IncludedIn(orderId, selectedId[i]))
                {
                    cashProd.transform.GetChild(0).gameObject.SetActive(true);
                }
                else
                {
                    cashProd.transform.GetChild(1).gameObject.SetActive(true);
                }
                yield return new WaitForSeconds(0.5f);
            }

            yield return new WaitForSeconds(0.5f);

            cashierCloud.SetActive(false);
            buyerCloud.SetActive(true);
            

            order[0].SetActive(false);
            order[1].SetActive(true);
            order[2].SetActive(false);

            orderCorrect = 0;
            for (int i = 0; i < orderSize; i++)
            {
                if (IncludedIn(orderId, selectedId[i])) orderCorrect++;
            }
            if(orderCorrect == orderSize)
            {
                data.money += 20 * orderSize;
                order[1].GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/emotionPositive");
                //PLAY MONEY SOUND
                if (data.setSound) musicMoney.Play();
            }else if(orderCorrect == 0)
            {
                order[1].GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/emotionNegative");
            }
            else
            {
                data.money += 10 * orderCorrect;
                order[1].GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/emotionNegative");
                //PLAY MONEY SOUND
                if (data.setSound) musicMoney.Play();
            }

            dataSave(data);
            MoneySet();


            buyer.GetComponent<Animator>().SetTrigger("goOutFromCashier");
            yield return new WaitForSeconds(3);

            order[1].SetActive(false);
            buyerCloud.SetActive(false);
            buyer.SetActive(false);

            for(int k = 0; k < orderSize; k++)
            {
                order[k].SetActive(false);
                cashierCloud.transform.GetChild(k).gameObject.transform.GetChild(0).gameObject.SetActive(false);
                cashierCloud.transform.GetChild(k).gameObject.transform.GetChild(1).gameObject.SetActive(false);
                cashierCloud.transform.GetChild(k).gameObject.SetActive(false);
                products[selectedId[k]].GetComponent<Image>().color = standartColor;
                products[selectedId[k]].transform.GetChild(0).gameObject.SetActive(false);
                selectedId[k] = -1;
            }

            yield return new WaitForSeconds(1);

            StartCoroutine(NewClient());

        }
    }


    void dataSave(SavedData data)
    {
        BinaryFormatter bf = new BinaryFormatter();

        if (File.Exists(saveFilePath))
        {
            file = File.OpenWrite(saveFilePath);
        }
        else
        {
            file = File.Create(saveFilePath);
        }

        bf.Serialize(file, data);
        file.Close();
        CheckMusic();
    }
    void dataLoad()
    {
        BinaryFormatter bf = new BinaryFormatter();

        if (File.Exists(saveFilePath))
        {
            try
            {
                file = File.OpenRead(saveFilePath);
                data = (SavedData)bf.Deserialize(file);
            }
            catch
            {
                data = new SavedData(500, false, false);
            }
        }
        else
        {
            file = File.Create(saveFilePath);
            data = new SavedData(500, false, false);
        }

        file.Close();
        CheckMusic();
    }

    void CheckMusic()
    {
        if (data.setMusic && !musicLoop.isPlaying)
        {
            musicLoop.Play();
        }else if (!data.setMusic)
        {
            musicLoop.Stop();
        }
    }

    void MoneySet()
    {
        textMoney.text = data.money.ToString();
    }

    void SetClickListeners()
    {
        buttonSell.onClick.AddListener(OrderProcessing);
        products[0].onClick.AddListener(() => SelectProduct(0));
        products[1].onClick.AddListener(() => SelectProduct(1));
        products[2].onClick.AddListener(() => SelectProduct(2));
        products[3].onClick.AddListener(() => SelectProduct(3));
        products[4].onClick.AddListener(() => SelectProduct(4));
        products[5].onClick.AddListener(() => SelectProduct(5));
        products[6].onClick.AddListener(() => SelectProduct(6));
        products[7].onClick.AddListener(() => SelectProduct(7));
        products[8].onClick.AddListener(() => SelectProduct(8));
        products[9].onClick.AddListener(() => SelectProduct(9));
        products[10].onClick.AddListener(() => SelectProduct(10));
        products[11].onClick.AddListener(() => SelectProduct(11));
        products[12].onClick.AddListener(() => SelectProduct(12));
        products[13].onClick.AddListener(() => SelectProduct(13));
        products[14].onClick.AddListener(() => SelectProduct(14));
        products[15].onClick.AddListener(() => SelectProduct(15));
        products[16].onClick.AddListener(() => SelectProduct(16));
        products[17].onClick.AddListener(() => SelectProduct(17));
        products[18].onClick.AddListener(() => SelectProduct(18));
        products[19].onClick.AddListener(() => SelectProduct(19));

        buttonSettigs.onClick.AddListener(OpenSettingsindow);
        settingsMusicButt.onClick.AddListener(MusicButton);
        settingsSoundButt.onClick.AddListener(SoundButton);
        settingsSaveButt.onClick.AddListener(SettingsSave);

    }


    void OpenSettingsindow()
    {
        //PLAY BUTTON SOUND
        if (data.setSound) musicButtonClick.Play();
        settingsWindow.SetActive(true);
        if (data.setMusic)
        {
            settingsMusicButt.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/SettingsOnButt");
        }
        else
        {
            settingsMusicButt.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/SettingsOffButt");
        }

        if (data.setSound)
        {
            settingsSoundButt.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/SettingsOnButt");
        }
        else
        {
            settingsSoundButt.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/SettingsOffButt");
        }
    }

    void SettingsSave()
    {
        dataSave(data);
        settingsWindow.SetActive(false);
    }

    void MusicButton()
    {
        //PLAY BUTTON SOUND
        if (data.setSound) musicButtonClick.Play();
        if (data.setMusic)
        {
            data.setMusic = false;
            settingsMusicButt.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/SettingsOffButt");
        }
        else
        {
            data.setMusic = true;
            settingsMusicButt.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/SettingsOnButt");
        }
    }
    void SoundButton()
    {
        //PLAY BUTTON SOUND
        if (data.setSound) musicButtonClick.Play();
        if (data.setSound)
        {
            data.setSound = false;
            settingsSoundButt.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/SettingsOffButt");
        }
        else
        {
            data.setSound = true;
            settingsSoundButt.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/SettingsOnButt");
        }
    }
}



[System.Serializable]
public class SavedData {

    public int money;
    public bool setSound;
    public bool setMusic;

    public SavedData(int money, bool setSound, bool setMusic)
    {
        this.money = money;
        this.setSound = setSound;
        this.setMusic = setMusic;
    }
}