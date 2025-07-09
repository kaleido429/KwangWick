// Web 해상도 컨트롤 함수
mergeInto(LibraryManager.library, {
  SetCanvasSizeByHeight: function () {
    console.log("[JS CALLED: SetCanvasSizeByHeight()]");

    function resizeCanvas() {
      const canvas = document.getElementById("unity-canvas");
      if (!canvas) {
        console.warn("⚠️ canvas not found");
        return;
      }

      const height = window.innerHeight;
      const width = height * (16 / 9);

      canvas.style.height = height + "px";
      canvas.style.width = width + "px";
      canvas.style.margin = "0 auto";
      canvas.style.display = "block";
      canvas.style.backgroundColor = "black";
    }

    // 최초 1회 실행
    resizeCanvas();

    // 브라우저 창 리사이즈할 때마다 자동 실행
    window.addEventListener("resize", resizeCanvas);
  }
});

