using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

[InitializeOnLoad]
internal static class PlayModeTestRunner
{
    private static readonly List<string> Logs=new List<string>();
    private static int _stage,_frames;
    private static bool _done,_loaded,_waveSeen;
    private static double _start;
    private static float _stageTime;
    private static GameObject _boss,_panel;
    private static Component _attack;
    private static Animator _animator;
    private static SpriteRenderer _sprite;
    private static Rigidbody2D _body;
    private static Sprite _firstSprite;
    private static float _groundY;
    static PlayModeTestRunner(){if(SessionState.GetString("PlayModeTest.State","Idle")=="EnteringPlayMode")EditorApplication.playModeStateChanged+=OnState;}
    public static void Arm(){_stage=0;_frames=0;_done=false;_loaded=false;_waveSeen=false;Logs.Clear();EditorApplication.update-=Enter;EditorApplication.update+=Enter;}
    private static void Enter(){EditorApplication.update-=Enter;SessionState.SetString("PlayModeTest.State","EnteringPlayMode");EditorApplication.playModeStateChanged-=OnState;EditorApplication.playModeStateChanged+=OnState;EditorApplication.isPlaying=true;}
    private static void OnState(PlayModeStateChange state){if(state!=PlayModeStateChange.EnteredPlayMode)return;EditorApplication.playModeStateChanged-=OnState;SessionState.SetString("PlayModeTest.State","InPlayMode");_start=EditorApplication.timeSinceStartup;Application.logMessageReceived+=Capture;EditorApplication.update+=Tick;}
    private static void Capture(string text,string stack,LogType type){if(Logs.Count<75&&(text.Contains("[Test]")||type==LogType.Error||type==LogType.Exception))Logs.Add(type+": "+text);}
    private static GameObject Named(string name){return SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).First(t=>t.name==name).gameObject;}
    private static void Invoke(Component c,string name,params object[] args){c.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).Invoke(c,args);}
    private static void Assert(bool value,string message){Debug.Log("[Test] "+message+"="+value);if(!value)throw new Exception(message);}
    private static GameObject[] Orbs(){return SceneManager.GetActiveScene().GetRootGameObjects().Where(g=>g.GetComponent("BossOrb")!=null).ToArray();}
    private static bool State(string name){return _animator.GetCurrentAnimatorStateInfo(0).IsName(name);}
    private static void Next(){_stage++;_stageTime=Time.time;}
    private static void Tick()
    {
        if(_done||++_frames<5)return;EditorApplication.QueuePlayerLoopUpdate();
        try{
            if(!_loaded){_loaded=true;SceneManager.LoadScene("Minigame_Espiritual");_stageTime=0f;return;}
            if(_stage==0){
                if(Named("Boss").GetComponent<Animator>()==null)return;
                ((Behaviour)Named("BossFightIntro").GetComponent("BossFightIntro")).enabled=false;
                _boss=Named("Boss");_panel=Named("VictoryPanel");_attack=_boss.GetComponent("BossAttack");_animator=_boss.GetComponent<Animator>();_sprite=_boss.GetComponent<SpriteRenderer>();_body=_boss.GetComponent<Rigidbody2D>();
                ((Behaviour)_attack).enabled=false;Invoke(Named("Player").GetComponent("PlayerHealth"),"PreventDamage");_firstSprite=_sprite.sprite;Next();return;
            }
            float elapsed=Time.time-_stageTime;
            if(_stage==1&&elapsed>0.25f){
                Assert(State("Idle"),"Grounded boss uses waiting animation");Assert(_sprite.sprite!=_firstSprite,"Waiting animation advances frames");
                Assert(_sprite.flipX&&Named("ShootPoint").transform.position.x<_boss.transform.position.x,"Sprite and fire point face player");
                var collider=_boss.GetComponent<BoxCollider2D>();_groundY=Named("Ground").GetComponent<BoxCollider2D>().bounds.max.y;
                Debug.Log("[Test] Boss sprite="+_sprite.bounds.size+" collider="+collider.bounds.size+" feet="+collider.bounds.min.y+" ground="+_groundY);
                Assert(collider.bounds.size.x>1f&&collider.bounds.size.x<2f&&Mathf.Abs(collider.bounds.min.y-_groundY)<0.08f,"Collider fits boss body on ground");
                Invoke(_attack,"ShootFan");Next();
            }
            else if(_stage==2&&elapsed>0.1f){Assert(State("Shoot")&&Orbs().Length==0,"Shooting pose precedes projectile release");Next();}
            else if(_stage==3&&elapsed>0.26f){
                Assert(State("Shoot")&&Orbs().Length==3,"Three orbs released at firing frame");Debug.Log("[Test] Fire frame="+_sprite.sprite.name);
                string path="Assets/BossAnimationVerification.png";ScreenCapture.CaptureScreenshot(path);SessionState.SetString("PlayModeTest.ScreenshotPath",path);Next();
            }
            else if(_stage==4&&elapsed>0.55f){
                Assert(State("Idle"),"Shooting returns to waiting");foreach(var orb in Orbs())UnityEngine.Object.Destroy(orb);
                Invoke(_attack,"JumpTowardsPlayer");Next();
            }
            else if(_stage==5&&elapsed>0.25f){Assert(State("Jump")&&_body.position.y> -1f,"Jump animation follows physics");Next();}
            else if(_stage==6){
                if(SceneManager.GetActiveScene().GetRootGameObjects().Any(g=>g.GetComponent("GroundMound")!=null))_waveSeen=true;
                if(_waveSeen&&State("Idle")){
                    Assert(_waveSeen,"Landing still raises ground hazards");Assert(State("Idle"),"Landing returns to waiting");
                    Invoke(_boss.GetComponent("BossHealth"),"TakeDamage",20);Next();
                }
            }
            else if(_stage==7&&elapsed>0.08f){
                Assert(State("Jump")&&!_boss.GetComponent<BoxCollider2D>().enabled,"Defeated boss uses exit jump animation");Next();
            }
            else if(_stage==8&&_boss==null){Assert(_panel.activeInHierarchy,"Exit still destroys boss and shows victory");Finish(true,null);return;}
            if(EditorApplication.timeSinceStartup-_start>20f)throw new Exception("Timeout stage "+_stage);
        }catch(Exception ex){Finish(false,ex.ToString());}
    }
    [Serializable]private class Result{public bool success;public string error;public string[] logs;}
    private static void Finish(bool success,string error){_done=true;EditorApplication.update-=Tick;Application.logMessageReceived-=Capture;SessionState.SetString("PlayModeTest.Result",JsonUtility.ToJson(new Result{success=success,error=error,logs=Logs.ToArray()}));SessionState.SetString("PlayModeTest.State","Done");EditorApplication.isPlaying=false;}
}
