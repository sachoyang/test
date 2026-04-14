using System.Collections;
using UnityEngine;

public class BundleLoad : MonoBehaviour
{
    //번들을 받을 서버의 URL(주소)
    //서버에서 받을 때 210.122.7.164/cu 이런식으로 넣어주면 됨
    public string bundleURL;

    //번들의 버전 (Version)
    public int version;

    // Use this for initialization
    [System.Obsolete]
    void Start()
    {
        //코루틴 함수(시간 지연함수)를 사용하여 LoadAssetBundle() 함수를 실행
        StartCoroutine(this.LoadAssetBundle());
    }

    [System.Obsolete]
    IEnumerator LoadAssetBundle()
    {
        //cache 폴더에 AssetBundle 을 담아야 하므로 캐싱 시스템이 준비될때까지 기달림
        while (!Caching.ready)
        {
            yield return null;
        }

        /* 새 WWW 변수를 만들고  WWW.LoadFromCacheOrDownload(URL, 버전) 함수를 통하여 에셋번들을 다운로드하여
         * cache 폴더에 저장.
         * 
         * WWW.LoadFromCacheOrDownload() 함수를 사용하면 우선 cache 폴더에 같은 버전의 에셋 번들이 있는지 확인하여
         * 있는 경우 호출하고 없는 경우 URL로부터 다운로드 한다.
         */

        // 역시 불러와 준다.
        WWW www = WWW.LoadFromCacheOrDownload(this.bundleURL, this.version);

        //www에 값이 쓰이기까지 시간이 걸린다. (cache 참조 -> 서버접속하여 다운) 값이 쓰이기까지 기다리자.
        //로드나 다운로드가 끝나길 대기
        yield return www;

        //다운로드된 www에 물려있는 assetbundle을 Assetbundle 자료형으로 참조하자.
        AssetBundle bundle = www.assetBundle;

        for (int i = 0; i < 3; i++)
        {
            /* 참조한 assetbundle에서 비동기 방식으로 에셋을 불러온다
             * Cube 1, Cube 2, Cube 3 이 차례로 참조 될 것이다.
             */
            AssetBundleRequest request = bundle.LoadAssetAsync("Cube " + (i + 1), typeof(GameObject));

            // 완료 할때까지 대기
            yield return request;

            /*  다음 코드로도 불러올수 있다. 차이 점은 위의 Async 가 붙어있는 코드의 경우에는 Request에 
             *  불러와야 하며 비동기 형식이기 때문에 무거운 에셋번들을 불러올때 메인 스레드가 멈추는 것을
             *  방지 할 수있다. 대신 코드가 약간 더 무겁다.
             *  GameObject obj = bundle.LoadAsset("Cube " + (i + 1) ) as GameObject;
             * 
             */

            // 가독성을 위해 일단 리퀘스트로 따온 게임오브젝트를 참조
            GameObject obj = request.asset as GameObject;

            // Temp 변수에 오브젝트를 새로 Instantiate()하여 물려주자 (오브젝트 생성)
            GameObject Temp = Instantiate(obj) as GameObject;

            // 포지션을 설정. x 값이 -10, 0, 10 순으로 입력될 것임.
            Temp.transform.position = new Vector3(-5.0f + (i * 5), 0.0f, 0.0f);

        }

        // 꼭 번들을 Unload 해야만 한다. - 메모리 소모, 복수 인스턴스 방지 등등
        bundle.Unload(false);

        //웹(서버)로부터의 연결을 끊어 메모리를 해제. (많이 안쓰는 함수)
        //     www.Dispose();

        //에러 검출 방법
        if (www.error != null)
        {
            Debug.Log("fail :(");
        }

    }



}
