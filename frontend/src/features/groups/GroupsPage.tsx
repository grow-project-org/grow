import { useState } from 'react';
import type { GroupType } from '../../types';
import { useGarden } from '../../state/GardenContext';
import { useToast } from '../../state/ToastContext';
import { EXTRA_ACTIONS } from '../../domain/extraActions';
import { CheckIcon, PlusIcon } from '../../components/ui/icons';
import { IconButton } from '../../components/ui/IconButton';
import { MoreVerticalIcon } from '../../components/ui/icons';
import { CheckToggle } from '../../components/ui/CheckToggle';
import { ActionGridSheet } from '../../components/sheet/ActionGridSheet';
import { AddGroupSheet } from './AddGroupSheet';
import { PlantPickerSheet } from './PlantPickerSheet';
import { selectGroups, type GroupAction } from './groups.selectors';
import styles from './GroupsPage.module.css';

type Sheet =
  | { kind: 'actions'; group: string }
  | { kind: 'picker'; group: string }
  | { kind: 'addGroup' }
  | null;

export const GroupsPage = () => {
  const garden = useGarden();
  const { flash } = useToast();
  const [expanded, setExpanded] = useState<Record<string, boolean>>({});
  const [sheet, setSheet] = useState<Sheet>(null);

  const cards = selectGroups(garden.garden, garden.groups, garden.done, garden.dismissed);

  const toggleExpand = (key: string) =>
    setExpanded((cur) => ({ ...cur, [key]: !cur[key] }));

  const runAction = (action: GroupAction, ids: number[], message: string) =>
    garden.commitAction(ids, action.type, message);

  const activeGroup =
    sheet && sheet.kind !== 'addGroup' ? sheet.group : undefined;

  return (
    <div className={styles.page}>
      <header className={styles.header}>
        <h1 className={styles.heading}>Grupy</h1>
        <button type="button" className={styles.newBtn} onClick={() => setSheet({ kind: 'addGroup' })}>
          <PlusIcon size={14} />
          Nowa
        </button>
      </header>

      <div className={styles.list}>
        {cards.map((card) => (
          <article key={card.name} className={styles.card}>
            <div className={styles.cardHead}>
              <div className={styles.cardEmoji}>{card.emoji}</div>
              <div className={styles.cardText}>
                <div className={styles.cardName}>{card.name}</div>
                <div className={styles.cardMeta}>
                  <span className={styles.tag} style={{ background: card.tagBg, color: card.tagInk }}>
                    {card.typeLabel}
                  </span>
                  <span className={styles.memberSub}>{card.memberSub}</span>
                </div>
              </div>
              <IconButton soft aria-label="Więcej" onClick={() => setSheet({ kind: 'actions', group: card.name })}>
                <MoreVerticalIcon />
              </IconButton>
            </div>

            {card.isRegion && (
              <div className={styles.regionStats}>
                <div className={styles.statWater}>
                  <div className={styles.statNum}>{card.regionWaterDue}</div>
                  <div className={styles.statLabel}>💧 do podlania</div>
                </div>
                <div className={styles.statFert}>
                  <div className={styles.statNum}>{card.regionFertDue}</div>
                  <div className={styles.statLabel}>🌱 do nawożenia</div>
                </div>
              </div>
            )}

            {card.showWarning && (
              <div className={styles.warning}>
                <span className={styles.warningIcon}>⚠️</span>
                <div>
                  <div className={styles.warningTitle}>Mieszane harmonogramy</div>
                  <div className={styles.warningText}>
                    Rośliny mają różny rytm — akcja zbiorcza rzadko obejmie wszystkie. Zwykle warto
                    rozbić grupę.
                  </div>
                  <button type="button" className={styles.warningBtn} onClick={() => garden.dismissWarning(card.name)}>
                    Rozumiem, zignoruj
                  </button>
                </div>
              </div>
            )}

            {card.allClear && (
              <div className={styles.clear}>
                <CheckIcon size={18} />
                {card.clearLabel}
              </div>
            )}

            <div className={styles.actions}>
              {card.actions.map((action) => {
                const key = `${card.name}:${action.type}`;
                const isOpen = !!expanded[key];
                return (
                  <div key={action.type} className={styles.action}>
                    <div className={styles.actionHead}>
                      <div className={styles.actionTitle}>
                        <span className={styles.actionEmoji}>{action.emoji}</span>
                        <span>{action.label}</span>
                      </div>
                      <span className={styles.actionStat}>{action.headStat} dziś</span>
                    </div>

                    {action.none ? (
                      <div className={styles.actionNone}>
                        <CheckIcon size={15} />
                        Nic na dziś
                      </div>
                    ) : (
                      <div className={styles.actionButtons}>
                        <button
                          type="button"
                          className={styles.primaryBtn}
                          onClick={() =>
                            runAction(
                              action,
                              action.dueIds,
                              `${action.emoji} ${action.verb} potrzebujące (${action.due}) w „${card.name}”`,
                            )
                          }
                        >
                          {action.primaryBtn}
                        </button>
                        {action.partial && (
                          <button
                            type="button"
                            className={styles.allBtn}
                            onClick={() =>
                              runAction(
                                action,
                                action.allIds,
                                `${action.emoji} ${action.verb} wszystkie (${action.trackedCount}) w „${card.name}”`,
                              )
                            }
                          >
                            {action.allBtn}
                          </button>
                        )}
                      </div>
                    )}

                    {action.alignable && (
                      <button
                        type="button"
                        className={styles.alignBtn}
                        onClick={() =>
                          runAction(action, action.allIds, `🔁 Wyrównano rytm w „${card.name}”`)
                        }
                      >
                        🔁 Wyrównaj rytm
                      </button>
                    )}

                    <button type="button" className={styles.expandBtn} onClick={() => toggleExpand(key)}>
                      {isOpen ? 'Ukryj listę' : 'Które konkretnie?'}
                    </button>

                    {isOpen && (
                      <div className={styles.rows}>
                        {action.rows.map((row) => (
                          <div key={row.id} className={styles.row}>
                            <div className={styles.rowText}>
                              <div className={row.done ? styles.rowDone : styles.rowName}>{row.name}</div>
                              <div className={styles.rowSub}>{row.sub}</div>
                            </div>
                            <span className={styles.rowState}>{row.stateLabel}</span>
                            <CheckToggle
                              checked={row.done}
                              onClick={() => garden.toggleToday(row.id, action.type)}
                              size={30}
                              radius={9}
                            />
                          </div>
                        ))}
                      </div>
                    )}
                  </div>
                );
              })}
            </div>

            <div className={styles.footer}>
              <button type="button" className={styles.footerBtn} onClick={() => setSheet({ kind: 'picker', group: card.name })}>
                <PlusIcon size={14} />
                Rośliny w grupie
              </button>
            </div>
          </article>
        ))}
      </div>

      <ActionGridSheet
        open={sheet?.kind === 'actions'}
        onClose={() => setSheet(null)}
        kicker="Akcja dla grupy"
        title={activeGroup ?? ''}
        onAddPlants={() => activeGroup && setSheet({ kind: 'picker', group: activeGroup })}
        actions={EXTRA_ACTIONS.map((a) => ({
          emoji: a.emoji,
          label: a.label,
          onClick: () => {
            flash(`${a.emoji} ${a.label} · ${activeGroup ?? ''}`);
            setSheet(null);
          },
        }))}
      />

      <PlantPickerSheet
        open={sheet?.kind === 'picker'}
        onClose={() => setSheet(null)}
        groupName={activeGroup ?? ''}
        garden={garden.garden}
        onToggle={(id) => activeGroup && garden.toggleGroupMember(id, activeGroup)}
      />

      <AddGroupSheet
        open={sheet?.kind === 'addGroup'}
        onClose={() => setSheet(null)}
        onCreate={(name: string, type: GroupType) => {
          garden.addGroup(name, type, `📁 Dodano grupę „${name}”`);
          setSheet(null);
        }}
      />
    </div>
  );
};
