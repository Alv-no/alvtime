import fetch from "node-fetch";
import { logger } from "../../createLogger";
import tagsDB, { Tag } from "../../models/tags";
import mockTags from "./cvpartnerTags";

async function fetchBatchFromCVPartner(offset: number): Promise<Tag[]> {
  const token = process.env.CVPARTNER_API_TOKEN;
  const res = await fetch(
    `https://alv.cvpartner.com/api/v1/unapproved/no/technologies/tags?offset=${offset}`,
    { headers: { Authorization: `Token token=${token}` } }
  );

  const body = (await res.json()) as { wrapper: { terms: Tag[] } };
  return body.wrapper.terms;
}

export async function updateFromCVPartner() {
  logger.info("Starting update of tags form CVPartner");
  let offset = 0;
  let countFetchedLast = 100;
  do {
    const cvPartnerTags = await fetchBatchFromCVPartner(offset);
    const newTags: Tag[] = [];
    for (const tag of cvPartnerTags) {
      const exists = await tagsDB.exists(tag.term);
      if (!exists) newTags.push(tag);
    }
    const innsertedTags = await tagsDB.batchInsert(newTags);
    logger.info(`Saved mock ${innsertedTags.length} tags to DB`);
    countFetchedLast = cvPartnerTags.length;
    offset = offset + 100;
  } while (countFetchedLast === 100);
  logger.info("Finished updating tags from CVPartner");
}

export async function updateFromMockTags() {
  const tagsInDB = await tagsDB.count();
  logger.info(`Number of tags in DB is ${tagsInDB}`);
  if (tagsInDB) return;
  const innsertedTags = await tagsDB.batchInsert(mockTags);
  logger.info(`Saved mock ${innsertedTags.length} tags to DB`);
}
