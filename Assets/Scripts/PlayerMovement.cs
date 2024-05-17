using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;


public class PlayerMovement : MonoBehaviour
{
    public float speed;
    private float velocity;
    private Camera mainCam;
    public float roadEndPoint;

    private float camVelocity;
    public float camSpeed=0.4f;
    private Vector3 offset;

    private Transform player;
    private Vector3 firstMousePos, firstPlayerPos;
    private bool moveTheBall;

    public float playerzSpeed=15f;

    public GameObject bodyPrefab;
    public int gap = 2;
    public float bodySpeed = 15f;

    private List<GameObject> bodyParts = new List<GameObject>();
    private List<int> bodyPartsIndex = new List<int>();
    private List<Vector3> PositionHistory = new List<Vector3>();
    
    public float shakeDuration;
    public float shakePower;
    private bool isDead;
    void Start()
    {
        mainCam=Camera.main;
        player = this.transform;
        offset = mainCam.transform.position - player.position;
    }

    
    void Update()
    {
        if (isDead)
        {
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            moveTheBall = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            moveTheBall = false;
        }

        if (moveTheBall)
        {
            Plane newPlane = new Plane(Vector3.up,0.8f);
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

            if (newPlane.Raycast(ray,out var distance))
            {
                Vector3 newMousePos = ray.GetPoint(distance) - firstMousePos;
                Vector3 newPlayerPos = newMousePos + firstMousePos;
                newPlayerPos.x = Mathf.Clamp(newPlayerPos.x, -roadEndPoint, roadEndPoint);
                player.position = new Vector3(Mathf.SmoothDamp(player.position.x, newPlayerPos.x, ref velocity, speed),
                        player.position.y, player.position.z);
            }
        }
    }

    private void FixedUpdate()
    {
        if (isDead)
        {
            return;
        }
        player.position += Vector3.forward * playerzSpeed * Time.fixedDeltaTime;
        PositionHistory.Insert(0,transform.position);
        int index = 0;
        foreach (var body in bodyParts)
        {
            Vector3 point = PositionHistory[Mathf.Min(index * gap, PositionHistory.Count - 1)];
            Vector3 moveDir = point - body.transform.position;
            body.transform.position += moveDir * bodySpeed * Time.fixedDeltaTime;
            body.transform.LookAt(point);
            index++;
        }
    }
    private void LateUpdate()
    {
        Vector3 newCamPos = mainCam.transform.position;
        mainCam.transform.position = 
            new Vector3(Mathf.SmoothDamp(newCamPos.x, player.position.x, ref camVelocity, camSpeed ),
            newCamPos.y, player.position.z + offset.z);
    }

    public void GrowBody()
    {
        GameObject body = Instantiate(bodyPrefab, transform.position, transform.rotation);
        bodyParts.Add(body);
        int index = 0;
        index++;
        bodyPartsIndex.Add(index);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag=="CellObs")
        {   
            Destroy(other.gameObject,0.005f);
            GrowBody();
        }
        if (other.gameObject.tag=="Obs")
        {
            Hit();
            
        }
        // if (other.gameObject.tag == "Finish")
        // {
        //     
        // }
    }

    private Sequence deadSequence;

    public void Hit()
    {
        isDead = true;  
        deadSequence?.Kill();
        deadSequence=DOTween.Sequence();
        
        deadSequence.Append(Camera.main.DOShakeRotation(shakeDuration, shakePower));
        deadSequence.AppendInterval(1);
        deadSequence.OnComplete((() =>
        {
            SceneManager.LoadScene(0);
           
        }));
        
        // Camera.main.DOShakeRotation(shakeSure, shakeGucu).OnComplete((() =>
        // {
        //    StartCoroutine(WaitAndRestart());
        // }));
        
    }

    private IEnumerator WaitAndRestart()
    {
        yield return new WaitForSeconds(1);
    }
}
