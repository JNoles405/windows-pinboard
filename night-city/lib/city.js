// Scene text. Wrap one phrase per scene in [[double brackets]] — it becomes the
// hidden clue, and `hint` is what's revealed when the visitor taps it.

export const BANDS = {
  lamplight: { name: 'Lamplight', range: '7 PM – 10 PM' },
  longhour: { name: 'The Long Hour', range: '10 PM – 1 AM' },
  lowtide: { name: 'Lowtide', range: '1 AM – 4 AM' },
  thinning: { name: 'Thinning', range: '4 AM – 6 AM' },
};

export const WEATHER = {
  clear: 'Clear',
  rain: 'Rain',
  fog: 'Fog',
};

export const SCENES = {
  lamplight: {
    clear: {
      title: 'The Lamps Are Reminded',
      text: `Tallow Street unshutters itself from the top of the Lamp Stair down. Ottoline Marsh walks ahead of the dark with her brass key, lighting lamps that are already burning. "They forget," she tells you, not looking up. "Same as anyone." Halden props open the bakery door with a loaf too hot to hold. At the bottom of the hill, past the shoe-mender and the shop that only sells left gloves, the shutters of [[the Lost Property Office]] stay down. Someone has chalked a small arrow on them, pointing at the moon.`,
      hint: 'A note under the arrow: "Open at Lowtide. Bring nothing. You already left it here."',
    },
    rain: {
      title: 'Umbrellas for Rent',
      text: `Mr. Pell has set up under the awning of his map shop, renting umbrellas by the street. "This one is good for Tallow and half of Cinder Lane. Past that you're on your own." The rain runs downhill in thin silver ropes, all the way to the Sill, where it pools in the sunken square. Lanterns are going on down there, one by one, under the water. A sign at the top of the steps reads [[DROWNED MARKET — OPENING SOON]].`,
      hint: 'In smaller letters: "Fully open at Lowtide, if the rain holds. Stalls on boats. Bring a coin you don\'t need."',
    },
    fog: {
      title: 'The Stair Miscounts',
      text: `Fog comes up off the Sill and climbs the Lamp Stair, one step at a time. You count as you go down: two hundred and twelve, two hundred and thirteen, two hundred and nineteen. The street signs have quietly swapped places. Tallow Street is calling itself Wren Row, and Wren Row is not answering. Ottoline's lamps are yellow smudges, nothing more. Somewhere in the white, [[a bell rings four times]], though it is not yet eight.`,
      hint: 'The bell is early. It belongs to the Thinning. Come back before dawn, when the fog is leaving and takes things with it.',
    },
  },
  longhour: {
    clear: {
      title: 'Bread for Strangers',
      text: `Halden's shelves are full of loaves with names on paper tags: MARGOT, DEV, A WOMAN IN A GREEN COAT, YOU. You pick up the last one, and it's warm, and it's yours. "Baked it Thursday," he says. "Knew you'd come eventually." Across the canal, the red light on [[the Tower]] blinks out a slow rhythm that doesn't match any clock. People on Tallow Street stop and listen to it the way you'd listen for your name in a crowd.`,
      hint: 'The Tower only speaks clearly when the fog is in. Come back at Lowtide on a foggy night and stand still.',
    },
    rain: {
      title: 'The Ferry Runs Wet',
      text: `The night ferry crosses the Sill in the rain with no lamp at its bow. The Ferryman holds out his hand, and you realize the fare is the last thing you said aloud. You try to remember what it was. He waits. Behind you the Drowned Market is half open: a fishmonger selling bottled thunder, a girl who sharpens umbrellas. Your ferry token is stamped [[2 AM]] on one side and blank on the other.`,
      hint: 'Flip it at Lowtide. The blank side fills in. It names the stall that will be waiting for you.',
    },
    fog: {
      title: "Pell's Corrections",
      text: `Mr. Pell is at his drafting table, erasing. "A street moved," he says. "They do that in fog. Bad manners." His walls are papered with maps of Wick, each slightly wrong in a different way. One shows a staircase that goes up into the sky and stops. One shows the Sill flowing the wrong way. He tears off a small square of paper and hands it to you: [[a map of somewhere that isn't built yet]].`,
      hint: '"Keep it until you\'ve seen all of Tallow Street," Pell says. "Every hour, every weather. Then it\'ll be true."',
    },
  },
  lowtide: {
    clear: {
      title: 'The Lost Property Office',
      text: `The shutters are up. The Clerk sits behind a high counter under a single green lamp, surrounded by shelves of things people lost in dreams: a swimming medal, a house key to a house that burned in 1974, the feeling of being picked up as a child. "Name?" the Clerk asks, and writes something down before you answer. They slide a small cardboard box across the counter. [[Inside is something you recognize]] but can't place.`,
      hint: 'The tag reads: "Unclaimed. Returned by Tuesday." The cat. Find her in the fog.',
    },
    rain: {
      title: 'The Drowned Market',
      text: `The square is fully underwater now, and fully open. Stalls float on rowboats tied to the lampposts. You can buy a jar of someone's best summer, a hat that was forgotten on a train, the sound of a door you used to know. Prices are in coins you don't need. A man in a sou'wester sells nothing at all. He just sits and watches you very kindly. When you look back, his boat has drifted toward [[the far end of Tallow Street]].`,
      hint: 'He\'ll be waiting at the end of the street at the Thinning. You\'ll recognize the rain on his hat.',
    },
    fog: {
      title: 'Tuesday',
      text: `A grey cat steps out of the fog, looks at you once, and walks off as if you'd agreed to follow her. You follow. Down an alley Pell hasn't mapped yet, past a door with your initials on it, to the edge of the Sill. Across the water the Tower crackles, and in the static, just for a moment, [[someone says your name]]. Tuesday sits down beside you. She has done this many times before.`,
      hint: 'The voice sounded like it was calling from earlier in the night. From Lamplight. From somewhere clear.',
    },
  },
  thinning: {
    clear: {
      title: 'Ottoline Goes Home',
      text: `Ottoline walks Tallow Street in reverse, uphill, putting the lamps out one by one with a kind word to each. Behind her, the street is already becoming less certain. The shoe-mender's sign is blank. The left-glove shop is a wall. "It's not sad," she says, seeing your face. "It's tidying." At the top of the Lamp Stair she hands you [[a brass key, still warm]] and doesn't explain.`,
      hint: 'It fits one lamp: the first lamp on Tallow Street at Lamplight. Light it yourself next time.',
    },
    rain: {
      title: 'The Color Runs',
      text: `The rain is washing the color out of Wick. The red door of Halden's runs pink into the gutter. The green of the Clerk's lamp drips off the lampshade. Everything is turning the grey of a pencil sketch. At the bottom of the street the man in the sou'wester is waiting. He gives you a folded umbrella and says only, "For the next time it rains." On the handle someone has carved [[a small loaf of bread]].`,
      hint: 'Halden has another loaf with your name on it. Come back during the Long Hour on a clear night.',
    },
    fog: {
      title: 'Last Ferry',
      text: `This is the last crossing. The fog is so thick that the Ferryman steers by sound. The Tower hums a single long note, and the city answers it, a bell here, a shutter there, as it folds itself up for the day. When you reach the far bank, there is no far bank, only morning arriving. In your pocket you find [[a map with wet edges]]. It shows Tallow Street exactly as it was tonight.`,
      hint: '"Come earlier," someone has written in the margin. "Come when it\'s raining. The ferry runs at the Long Hour."',
    },
  },
};

export const DAY = {
  title: 'Wick Is Not Here',
  text: `There is an empty lot where Tallow Street should be. A few weeds. A lamppost with no lamp. If you look closely, there is a chalk arrow on the pavement, worn almost away, pointing west, toward where the sun will set.`,
};
