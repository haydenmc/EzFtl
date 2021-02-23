import { FtlPlayer } from "janus-ftl-player";

window.addEventListener("load", () => {
    let videoElements = document.querySelectorAll("video.ftl");
    videoElements.forEach(element => {
        let channelId = element.getAttribute("data-channel-id");
        let janusUri = element.getAttribute("data-janus-uri");
        if ((channelId !== null) && (janusUri !== null))
        {
            let player = new FtlPlayer(element as HTMLVideoElement, janusUri);
            player.init(Number(channelId));
        }
    });
});