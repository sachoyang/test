using UnityEngine;
using System.Collections;

public class PlayerCtrl : MonoBehaviour {
	
	private Animator anim;					// Player 객체의 Animator component 를 위한 Reference(참조) 이다
	private Rigidbody2D rigidbody2D;        // Player 객체의 Rigidbody2D 를 위한 Reference(참조) 이다


    // 인스펙터에 노출 안됨 
    [HideInInspector]
	public bool dirRight = true;			// 플레이어의 현재 바라보는 방향을 알기 위함 


	[HideInInspector]
	public bool jump = false;	   			// 플레이어가 현재 점프중인지 아닌지 알기 위함 
	public float jumpForce = 1000f; 		// 플레이어가 점프를 할때의 추가되는 힘의 양 
	public AudioClip[] jumpClips;  			// 플레이어의 여럿 점프 사운드를 위한 오디오 클립 배열 

	
	private bool grounded = false;			// 플레이어가 땅에 있는지 아닌지 구별을 위한 변수
	private Transform groundCheck;	 		// 만약 플레이어가 땅에 있을때 position을 marking 할 곳

	
	public float moveForce = 365f;			// 플레이어의 왼쪽 오른쪽 이동을 위한 추가되는 힘의 양	
	public float maxSpeed = 5f;				// 가장 빨르게 x 축 안에서 플레이어가 이동 할수있는 최고 스피드

	 
	public float tauntProbability = 50f;	// 플레이어가 적을 조롱하게 기회 제공을 위한 변수 
	public AudioClip[] taunts;				// 플레이어가 적을 조롱 할때를 위한 오디오 클립 배열 
	private int tauntIndex;	  				// 가장 최근에 플레이된(조롱) taunts 배열의 Index의 저장을 위한 변수 
	public float tauntDelay = 1f; 			// 조롱이 발생 할때 딜레이를 줘야만 한다. 안그러면 사운드가 중복 된다.
				
	
	void Awake()
	{
		// 레퍼런스(참조)들을 셋팅.
		groundCheck = transform.Find("groundCheck");
		anim = GetComponent<Animator>();
        rigidbody2D = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
		// 디버깅용 링크에러 체크
		Debug.Assert(groundCheck);
        Debug.Assert(anim);
        Debug.Assert(rigidbody2D);
    }

    void Update()
	{
		// 플레이어 position 으로부터 groundcheck position 까지 linecast(두 점을 잇는 선을 그림 )할때 
		// 만약 충돌한 어떤객체가 Ground layer 라면 현재 플레이어는 땅에 있는거다.
		grounded = Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground"));  

		// 만약 점프 버튼을 눌렀를때 플레이어가 땅에 있었다면 플레이어는 점프 할 수있다.
		if(Input.GetButtonDown("Jump") && grounded)
			jump = true;
	}


	//고정 시간마다 호출 
	void FixedUpdate ()
	{
		//Input클래스안에 다음 GetAxis() 함수 호출로 horizontal 입력을 캐치한다.
		float h = Input.GetAxis("Horizontal");

		// animator 컴포넌트에 parameter(매개변수)인 Speed에 horizontal(수평) 입력값의 절대값(Mathf.Abs())을 셋팅한다.
		anim.SetFloat("Speed", Mathf.Abs(h));

		//만약 플레이어의 바라보는 방향이 바뀌거나 혹은 maxSpeed에 아직 도달하지 않을때 ( h(-1.0f~1.0f)는 velocity.x를 다르게 표시한다)
		if(h * rigidbody2D.velocity.x < maxSpeed)
            //플레이어 객체에 힘을 가한다.
            rigidbody2D.AddForce(Vector2.right * h * moveForce);

		// 만약에 플레이어의 수평 속도가 maxSpeed 보다 크면 
		if(Mathf.Abs(rigidbody2D.velocity.x) > maxSpeed)
            //플레이어의 velocity(속도)를 x축방향으로 maxSpeed 로 셋팅해줘라 또한 기존 rigidbody2D.velocity.y 도 셋팅 해 줘야 한다.
            // Mathf.Sign() 는 매개변수를 참조해서 1 또는 -1(float)을 반환  
            rigidbody2D.velocity = new Vector2(Mathf.Sign(rigidbody2D.velocity.x) * maxSpeed, rigidbody2D.velocity.y);

		// 만약 플레이어가 왼쪽을 바로볼때 플레이어를 오른쪽으로 이동하게 입력했다면 
		if(h > 0 && !dirRight)
			// 플레이어를 뒤집어라
			Flip();
		// 그렇지 않고 만약 플레이어가 오른쪽을 바로볼때 플레이어를 왼쪽으로 이동하게 입력했다면 
		else if(h < 0 && dirRight)
			// 플레이어를 뒤집어라
			Flip();
		
		// 만약 플레이어가 점프를 한다면
		if(jump)
		{
			// animator의 trigger(전환) parameter에 Jump를 셋팅
			anim.SetTrigger("Jump");

			// jump audio clip이 랜덤으로 플레이된다.
			int i = Random.Range(0, jumpClips.Length);
			AudioSource.PlayClipAtPoint(jumpClips[i], transform.position);

			//플레이어게게 수직 힘이 가해진다
			rigidbody2D.AddForce(new Vector2(0f, jumpForce));

			// Update에서 조건이 만족하여 점프상태가 될때까지 확실하게 플레이어가 다시 점프를 못하게 만들어라 
			jump = false;
		}
	}
	

	// 케릭터의 현재 방향을 바꿔주는 함수 
	void Flip ()
	{
		//플레이어의 바라보는 방향을 바꾸자 
		dirRight = !dirRight;

		//플레이어의 local scale x에 -1을 곱하자
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}
	

	// 조롱 함수 
	public IEnumerator Taunt()
	{
		//조롱의 랜덤찬스를 체크하자 
		float tauntChance = Random.Range(0f, 100f);
		if(tauntChance > tauntProbability)
		{
			//tauntDelay 초수만큼 기달려라 
			yield return new WaitForSeconds(tauntDelay);

			//만약 현재 플레이중인 클립이 없을 때 
			if(!GetComponent<AudioSource>().isPlaying)
			{
				//전에 플레이했던 사운드와 다른 조롱 사운드를 랜덤하게 선택하자
				tauntIndex = TauntRandom();

				//새로운 조롱 사운드를 플레이하자 
				GetComponent<AudioSource>().clip = taunts[tauntIndex];
				GetComponent<AudioSource>().Play();
			}
		}
	}
	

	//중복 안되고 랜덤하게 조롱 사운드를 선택하는 함수 
	int TauntRandom()
	{
		//taunts 배열의 랜덤 인덱스를 선택하자 
		int i = Random.Range(0, taunts.Length);

		//만약 이전의 조롱사운드와 같은면
		if(i == tauntIndex)
			// 다른 랜덤 조롱사운드 셋팅을 위해 다시 TauntRandom() 를 호출 하자 
			return TauntRandom();
		else
			//그렇지 않으면 이 인덱스를 리턴하자
			return i;
	}
}
