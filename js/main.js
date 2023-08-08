
import { RenderCanvas, renderCanvasMain } from './renderCanvas.js'; 




function main() {

    const useClass = true; 



    if (useClass) {
        let rc = new RenderCanvas('#c', 'mainContainer');
        rc.main();
    } else {
        renderCanvasMain();
    }
    
}


main();