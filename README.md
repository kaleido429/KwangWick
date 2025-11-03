# FPS 게임을 모방한 사격장
1인칭 고정 위치 캐릭터로 플레이 합니다.  
30초 동안 최대한 많은 target을 처치해서 높은 점수를 얻어야 합니다.  
단순한 kill이 아니라 명중률과 headshot 비율도 같이 기록합니다.
  
target은 여러 스폰 장소중에서 랜덤하게 스폰됩니다.  
target은 2종류로  
-장애물 뒤에서 빠르고 조금씩 움직이는 peeking target  
-멀리서 천천히 길게 이동하는 moving target
  
각 target으로 플레이어의 유저의 순발력과 정확도를 
플레이 영상과 해당 게임의 메타 데이터는 Firebase Storage에 업로드 됩니다.  

  Firebase Storage |
  /videos  
플레이 영상  720p, 30fps  
파일 이름(UUID),  
{크기, 유형(확장자), 업로드 날짜}  

 Firebase Database |  
 /game_results  
 파일 이름(UUID),  
 {accuracy, finalScore, movingHits, peekingHits, totalHeadshots, totalHits, totalShots}
