import { retrieve } from "./Store";

const LoadMultiple = async (plants: { id: number; links: string[] }[]) => {
  let loadedArr = [];
  for (let i = 0; i < plants.length; i++) {
    const plant = plants[i];
    let loaded = await Load(plant);
    loadedArr[i] = loaded;
  }
  return loadedArr;
}

const Load = async (images: { id: number; links: string[] }) => {
  var user = retrieve();
  var token = user.token;
  let loadedArr = [];
  for (let i = 0; i < images.links.length; i++) {
    const link = images.links[i];
    let loaded = await LoadImage(link, token as string);
    loadedArr[i] = loaded;
  }
  return {
    id: images.id,
    links: loadedArr
  };
}

const LoadImage = async (src: string, token: string) => {
  const options = {
    headers: {
      'Authorization': 'Bearer ' + token
    }
  };

  let res = await fetch(src, options);
  let blob = await res.blob();
  return URL.createObjectURL(blob);
}

export { Load, LoadMultiple };