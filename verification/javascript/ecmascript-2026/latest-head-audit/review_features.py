import collections
import importlib.util
import json
from pathlib import Path
import zipfile

here = Path(__file__).resolve().parent
repo = here.parents[2]
spec = importlib.util.spec_from_file_location("prepare", repo / "tools/Test262/prepare_test262.py")
prepare = importlib.util.module_from_spec(spec)
spec.loader.exec_module(prepare)
tags = {
    "Iterator.prototype.join": "proposal-iterator-join",
    "iterator-chunking": "proposal-iterator-chunking",
    "iterator-includes": "proposal-iterator-includes",
}
counts = collections.Counter()
examples = {}
with zipfile.ZipFile(here / "test262-d53ebc8b1931d9f1541e43d59650776e4b766248.zip") as bundle:
    for name in bundle.namelist():
        if "/test/built-ins/Iterator/" not in name or not name.endswith(".js") or "_FIXTURE" in name:
            continue
        try:
            metadata = prepare.frontmatter(bundle.read(name).decode("utf-8"))
        except ValueError:
            continue
        for tag in set(metadata.get("features", [])) & set(tags):
            counts[tag] += 1
            examples.setdefault(tag, name.split("/test/", 1)[1])
review = {
    "newFeatureTagsWithinSelectedCorpus": [],
    "unknownSelectedFlags": [],
    "newFeatureRegistryTags": [
        {"feature": tag, "filesInIteratorBuiltins": counts[tag], "example": examples.get(tag),
         "source": "https://github.com/tc39/" + tags[tag],
         "decision": "outside selected language/annexB/RegExp paths; methods use existing member/call syntax"}
        for tag in sorted(tags)
    ],
    "addedFutureSyntaxFilesAlreadyExcluded": {"import-defer": 2, "explicit-resource-management": 6},
    "unreviewedNewSyntaxTagsRequiringFilterChange": [],
}
(here / "feature-review.json").write_text(json.dumps(review, indent=2) + "\n", encoding="utf-8")
print(json.dumps(review, indent=2))
