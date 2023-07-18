import * as THREE from 'three';
import { TrackballControls } from "https://cdn.skypack.dev/three-trackballcontrols-ts@0.2.3";
import {OBJLoader} from 'three/addons/loaders/OBJLoader.js';
import { MeshStandardMaterial } from 'three';

const rootPath = "https://odeedelessons.org.ohio-state.edu/U.OSU.EDU/ARVR/WebglEmbed"; 

function resizeRendererToDisplaySize(renderer) {
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

function std135Fov(focalLength) {
    return Math.atan(18 / focalLength) * 2 * (180 / Math.PI); 
}

function main() {
    const canvas = document.querySelector('#c');
    const renderer = new THREE.WebGLRenderer({ antialias: true, canvas });
    renderer.setClearColor(0x000000, 0); // Turn background transparent 
	document.body.appendChild(renderer.domElement); // To use later for trackball 

	// Set up the camera 
    const fov = std135Fov(50);
    console.log(fov);
    const aspect = 2;  // the canvas default
    const near = 0.1;
    const far = 200;
    const camera = new THREE.PerspectiveCamera(fov, aspect, near, far);
    camera.position.z = 100;


	// Create the light 
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

    const loader = new THREE.TextureLoader();
    const pbrMaterial = new MeshStandardMaterial({
        map: loader.load(rootPath + '/public/textures/SkullDiffuse.jpg'),
        normalMap: loader.load(rootPath + '/public/textures/SkullNormal.jpg'),
        normalScale: new THREE.Vector2(1, 1),
        emissiveMap: loader.load(rootPath + '/public/textures/SkullDiffuse.jpg'),
        emissiveIntensity: .5
    });
    const listOfObj = ['PartBase.obj', 'PartFrontal.obj', 'PartNasal.obj', 'PartParitalLeft.obj', 'PartParitalRight.obj']
    const objLoader = new OBJLoader();
    for (let path of listOfObj) {
        objLoader.load(rootPath + '/public/models/' + path,
            function (obj) {
                obj.traverse(function (child) {
                    if (child instanceof THREE.Mesh) {
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
		
	
    function render(time) {
		let rotationScale = .2; 
        time *= (0.001 * rotationScale);  // convert time to seconds and scale it

        cube.rotation.x = time * rotationScale;
        cube.rotation.y = time * rotationScale;

        const canvas = renderer.domElement;
        camera.aspect = canvas.clientWidth / canvas.clientHeight;
        camera.updateProjectionMatrix();

        if (resizeRendererToDisplaySize(renderer)) {
            const canvas = renderer.domElement;
            camera.aspect = canvas.clientWidth / canvas.clientHeight;
            camera.updateProjectionMatrix();
        }
		
        requestAnimationFrame(render);
		controls.update();
		
		renderer.render(scene, camera);
    }

    requestAnimationFrame(render);
}


main();