import fetch from "node-fetch";
import { logger } from "../../createLogger";
import tagsDB, { Tag } from "../../models/tags";

async function fetchBatchFromCVPartner(offset: number): Promise<Tag[]> {
  const token = process.env.CVPARTNER_API_TOKEN;
  const res = await fetch(
    `https://alv.cvpartner.com/api/v1/unapproved/no/technologies/tags?offset=${offset}`,
    { headers: { Authorization: `Token token=${token}` } }
  );

  const body = await res.json();
  return body.wrapper.terms;
}

export async function updateFromCVPartner() {
  logger.info("Starting update of tags form CVPartner");
  const savedTags = await tagsDB.getAll();
  let offset = 0;
  let countFetchedLast = 100;
  do {
    const cvPartnerTags = await fetchBatchFromCVPartner(offset);
    for (const tag of cvPartnerTags) {
      const exists = savedTags.some((savedTag) => savedTag.term === tag.term);
      if (!exists) {
        const savedTag = await tagsDB.save(tag);
        logger.info(`Saved tag: ${savedTag.term}`);
      }
    }
    countFetchedLast = cvPartnerTags.length;
    offset = offset + 100;
  } while (countFetchedLast === 100);
  logger.info("Finished updating tags from CVPartner");
}
