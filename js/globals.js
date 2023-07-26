


// ---------------------------------------------------------------------
// -------------------------- Class definition -----------------------
// ---------------------------------------------------------------------
// For ease of use later in the scripts. 


class textureSet {
    constructor(diffuse, normal, roughness, metallic) {
        this.diffuse = diffuse;
        this.normal = normal;
        this.roughness = roughness;
        this.metallic = metallic; 
    }
}

// ---------------------------------------------------------------------
// -------------------------- Explicit variables -----------------------
// ---------------------------------------------------------------------
// Variables that may be used externally when adding the export keyword. 


// Path of the root folder in the server 
const rootPath = "https://odeedelessons.org.ohio-state.edu/U.OSU.EDU/ARVR/WebglEmbed"; 

const modelSubFolder = '/public/models/'; 

const textureMapSubFodler = '/public/textures/';

export const MonkeySkullMaps = new textureSet(
    rootPath + textureMapSubFodler + 'SkullDiffuse.jpg', 
    rootPath + textureMapSubFodler + 'SkullNormal.jpg',
    null, null, null
);



// ---------------------------------------------------------------------
// -------------------------- Implicit variables -----------------------
// ---------------------------------------------------------------------
// Variables that should not have the export keyword and should only 
// be used within this file for storage or other purposes. 

// const _PartsNamesWhatever 

const _PartsNamesMonkeySkull = [
    'PartBase.obj',
    'PartFrontal.obj',
    'PartNasal.obj',
    'PartParitalLeft.obj',
    'PartParitalRight.obj'
]



// ---------------------------------------------------------------------
// ----------------------------- Functions -----------------------------
// ---------------------------------------------------------------------


export function getMonkeySkullPartsPaths() {
    // Return the path to every parts of the monkey skull. 
    let pathList = [];
    for (let name of _PartsNamesMonkeySkull) {
        pathList.push(rootPath + modelSubFolder + name);
    }
    return pathList; 
}


export function std135Fov(focalLength) {
    // Given a focal length in mm, convert it into field of view,
    // assuming it is used on an standard 135 format still camera. 
    return Math.atan(18 / focalLength) * 2 * (180 / Math.PI);
}

