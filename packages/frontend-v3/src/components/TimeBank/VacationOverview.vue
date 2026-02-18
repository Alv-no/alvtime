<template>
	<div
		v-if="!loading"
	>
		<h2>
			Betalte feriedager
			<div class="tooltip">
				&#9432;
				<span class="tooltiptext">
					<strong>Feriepenger og feriedager, hva er det?</strong>

					<p><strong>Feriepenger:</strong> I løpet av et arbeidsår opptjener man feriepenger som utbetales året etter. Utgangspunktet er at man ikke har rett på lønn så lenge man ikke jobber (ferie), men for at samfunnet skal gå rundt har vi i Norge laget en system som sier at du i løpet av et år kan tjene deg opp penger for dager du har ferie året etter. I Alv er dette 12% av lønnen din.</p>

					<p>Jobber du et helt år i Alv, med en årslønn på 500 000,- tjener du deg opp 60 000 i feriepenger. Dette er penger som skal kompensere ferien du tar ut året etter (dager du ikke jobber). Det trekkes ikke skatt når feriepengene utbetales, som gjør at hvert år blir feriepengeutbetalingen større enn en vanlig lønnsutbetaling.</p>

					<p><strong>Feriedager:</strong> Feriedager er noe du har rett på, og ikke noe du opparbeider deg på samme måte. Alle i Alv har rett på 5 uker ferie, altså 25 dager. Alle disse dagene trekkes pengemessig i juni - uavhengig om du har jobbet i Alv hele fjoråret og opptjent alle feriepengene her.</p>

					<p>Har du jobbet i Alv i 6 mnd, og Selskap B 6 mnd, vil feriepengene du fikk utbetalt fra Selskap B dekke opp for de månedene du ikke jobbet i Alv. Eneste unntaket er hvis arbeidstaker kan bevise at det ikke er opptjent nok feriepenger året før til å dekke 25 dager ferie. Typisk tilfelle her er hvis Alv er den første jobben du har, og du begynner i august.</p>

					<strong>Så hvordan fungerer egentlig dette med feriepenger og feriedager?</strong>

					<p><strong>For dere som jobbet i Alv i hele fjor:</strong><br />
						I Juni får alle i Alv utbetalt feriepenger opptjent i fjor, samtidig som man trekkes for feriedager dette året. Lønnsslippen ser derfor slik ut:</p>
					<ul>
						<li>Brutto månedslønn</li>
						<li>+ Feriepenger</li>
						<li>- Trekk i lønn for feriedager</li>
						<li>- Korreksjon månedslønn og antall feriedager</li>
						<li>= Utbetaling juni</li>
					</ul>
					<p>
						<em>Brutto månedslønn</em> = Ny lønn / 12<br />
						<em>Feriepenger</em> = Feriepenger til gode fra i fjor (Brutto lønn i fjor * 0,12)<br />
						<em>Trekk i lønn for feriedager</em> = Dager som inngår i en måned (Regnet snitt til 21,67)<br />
						<em>Korreksjon månedslønn og antall feriedager</em> = Trekk for den delen av feriedager som går ut over en måned (25 - 21,67)
					</p>

					<p><strong>For dere som ikke har hatt jobb tidligere, eller gjort avtale om færre dager trekk:</strong><br />
						Du vil fortsatt få utbetalt opptjente feriepenger for i fjor, men i stedet for å bli trukket 25 feriedager, trekkes feriedager i samsvar med summen av feriepenger. Lønnsslippen ser slik ut:</p>
					<ul>
						<li>Brutto månedslønn</li>
						<li>+ Feriepenger opptjent i Alv i fjor (Brutto lønn i fjor * 0,12)</li>
						<li>- Trekk i lønn for ferie</li>
						<li>= Utbetalt juni</li>
					</ul>
					<p>
						<em>Brutto månedslønn</em> = Ny lønn / 12<br />
						<em>Trekk i lønn for ferie</em>: Antall dager ferie = Feriepenger til gode i fjor / Dagslønn<br />
						<em>Dagslønn</em> = (Ny årslønn / 12) / 21,67<br />
						<em>Trekket i lønn for ferie</em> = Dagslønn * Antall dager trekk for ferie
					</p>

					<p><strong>For dere som har jobbet et annet sted deler eller hele fjoråret:</strong><br />
						Dere vil fortsatt bli trukket for 25 dager ferie i Alv, men dere har mottatt hele eller deler av feriepengene fra tidligere arbeidsgiver.</p>

					<p><em>Husk å skille mellom feriepenger og feriedager!</em></p>
				</span>
			</div>
		</h2>
		<SectionedBar :sections="vacationSections" />
	</div>
</template>

<script setup lang="ts">
import { onMounted, ref, computed } from "vue";
import { storeToRefs } from "pinia";
import { useVacationStore } from "@/stores/vacationStore";
import SectionedBar from "./SectionedBar.vue";

const loading = ref<boolean>(true);

const vacationStore = useVacationStore();
const { vacation } = storeToRefs(vacationStore);

const vacationSections = computed(() => {
	return [
		{
			title: "Brukt",
			amount: vacation.value?.usedVacationDaysThisYear || 0,
			color: "yellow",
		},
		{
			title: "Tilgjengelig",
			amount: vacation.value?.availableVacationDays || 0,
			color: "green",
		},
		{
			title: "Planlagt",
			amount: vacation.value?.plannedVacationDaysThisYear || 0,
			color: "blue",
		}
	];
});

onMounted( async () => {
	await vacationStore.getVacationOverview();
	loading.value = false;
});
</script>

<style scoped lang="scss">
h2 {
	margin-top: 32px;
	margin-bottom: 8px;
}

.tooltip {
  position: relative;
  display: inline-block;
  border-bottom: 1px dotted black; /* Add dots under the hoverable text */
  cursor: pointer;
  vertical-align: middle;
  font-size: 0.9em;
}

.tooltiptext {
  visibility: hidden;
  width: 500px;
  max-height: 400px;
  overflow-y: auto;
  background-color: #333;
  color: #ffffff;
  padding: 16px;
  border-radius: 8px;
  position: absolute;
  z-index: 1;
  font-size: 14px;
  line-height: 1.5;

  p {
    margin: 12px 0;
  }

  ul {
    margin: 8px 0;
    padding-left: 20px;
  }

  li {
    margin: 4px 0;
  }

  strong {
    color: #ffd700;
  }

  em {
    color: #87ceeb;
  }
}

/* Show the tooltip text on hover */
.tooltip:hover .tooltiptext {
  visibility: visible;
}
</style>