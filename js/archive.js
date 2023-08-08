



// ---------------------------------------------------------------------
// ---------------------------------------------------------------------
// ---------------------------------------------------------------------
// ---------------------------------------------------------------------
// ---------------------------------------------------------------------

// Old redner canvas way 

function resizeRendererToDisplaySize(renderer) {
    // Resize the render area to accommodate the change of resoultion
    const canvas = renderer.domElement;
    const pixelRatio = window.devicePixelRatio;
    const width = canvas.clientWidth * pixelRatio | 0;
    const height = canvas.clientHeight * pixelRatio | 0;
    const needResize = canvas.width !== width || canvas.height !== height;
    if (needResize) {
        renderer.setSize(width, height, false);
    }
    return needResize;
}


export function renderCanvasMain() {

    // Accquiring the element and set it as the render target 
    const canvas = document.querySelector('#c');
    const renderer = new THREE.WebGLRenderer({ antialias: true, canvas });

    renderer.setClearColor(0x000000, 0); // Turn background transparent 
    document.getElementById("mainContainer").appendChild(renderer.domElement); // To use later for trackball 

	// Set up the camera 
    const fov = std135Aov(50);
    const aspect = 2;  // the canvas default
    const near = 0.1;
    const far = 2000;
    const camera = new THREE.PerspectiveCamera(fov, aspect, near, far);
    camera.position.z = 100;

	// Create the lights 
    const color = 0xFFFFFF;
    const intensity1 = 1;
    const light1 = new THREE.DirectionalLight(color, intensity1);
    light1.position.set(-1, 2, 4);
    const intensity2 = 0.75;
    const light2 = new THREE.DirectionalLight(color, intensity2);
    light2.position.set(1, -2, -4);

	// Create the scene and add the object and light 
    const scene = new THREE.Scene();
    scene.add(light1); 
    scene.add(light2); 

    // Load all the objects 
    const textureLoader = new THREE.TextureLoader();
    const pbrMaterial = new MeshStandardMaterial({
        map: textureLoader.load(MonkeySkullMaps.diffuse),
        normalMap: textureLoader.load(MonkeySkullMaps.normal),
        normalScale: new THREE.Vector2(1, 1),
        emissiveMap: textureLoader.load(MonkeySkullMaps.diffuse),
        emissiveIntensity: .5
    });

    var modelLoader = null; 
    if (useFBX)
        modelLoader = new FBXLoader();
    else
        modelLoader = new OBJLoader();

    for (let path of modelToUse()) {
        modelLoader.load(path,
            function (obj) {
                obj.traverse(function (child) {
                    if (child instanceof THREE.Mesh && useTextures) {
                        child.material = pbrMaterial;
                    }
                });
                scene.add(obj);
            },
            function (xhr) {
                console.log((xhr.loaded / xhr.total * 100) + "% loaded")
            },
            function (err) {
                console.error("Error loading 'ship.obj'")
            }
        )
    }
	

	// Create the trackball control 
	const controls = new TrackballControls(camera, renderer.domElement); 
	controls.rotateSpeed = 4;
	controls.dynamicDampingFactor = 0.1;
		
	
    function Update(time) {
		let rotationScale = .2; 
        time *= (0.001 * rotationScale);  // convert time to seconds and scale it

        const canvas = renderer.domElement;
        camera.aspect = canvas.clientWidth / canvas.clientHeight;
        camera.updateProjectionMatrix();

        if (resizeRendererToDisplaySize(renderer)) {
            const canvas = renderer.domElement;
            camera.aspect = canvas.clientWidth / canvas.clientHeight;
            camera.updateProjectionMatrix();
        }
		
        requestAnimationFrame(Update);
		controls.update();
		
		renderer.render(scene, camera);
    }

    requestAnimationFrame(Update);
}
