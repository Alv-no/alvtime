import { getRandomNumber } from "./reactions";

export const getResponseMessage = (messageType: string) => {
  switch (messageType) {
    case "whatUserIsLearningQuestion": {
      return whatUserIsLearningQuestion[
        getRandomNumber(whatUserIsLearningQuestion.length)
      ];
    }
    case "learnMoreText": {
      return `${
        learnMoreText[getRandomNumber(learnMoreText.length)]
      } :point_right:`;
    }
    case "boastAboutLearningText": {
      return boastAboutLearningText[
        getRandomNumber(boastAboutLearningText.length)
      ];
    }
  }
};

const whatUserIsLearningQuestion: Array<string> = [
  "Trykk på knappen og fortell meg hva du lærer deg for tiden :wave:",
  "Har du lest, sett eller snakket om noe fett i det siste? La folket få vite det! :bulb:",
  "Vår kunnskapsbrønn trenger påfyll og lengter etter din hjernebarks oppdaterte signaler! :brain:",
];

const learnMoreText: Array<string> = [
  "Sugen på en nærmere titt? Sjekk kilden!",
  "Nysgjerrig? Finn ut mer her!",
  "Vi vet du ble inspirert. Sjekk det ut nærmere her!",
];

const boastAboutLearningText: Array<string> = [
  "bygger kunnskap som bare det! :tada:",
  "har en skalle som renner over av ny kunnskap! :exploding_head:",
  "kompenserer med kompetitiv kompetanse!",
];

export function thanksForSharingText() {
  const texts = [
    "Takk for at du deler hva du leker med :thumbsup:",
    "Bra jobba! Du lærer, jeg lagrer :smile:",
    "Det er notert :thumbsup: Kult å se at du lærer deg nye ting!",
    "Ditt hode fylles stadig med smarte ting :thumbsup: Jeg har dyttet det inn i databasen",
  ];
  return getRandomFromArray(texts);
}

function getRandomFromArray(arr: string[]) {
  return arr[getRandomNumber(arr.length)];
}
